using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightsManager : IFlightsManager
    {
        private static ConcurrentDictionary<string, FlightPlan> flightPlans =
            new ConcurrentDictionary<string, FlightPlan>();
        private static ConcurrentDictionary<string, Server> servers =
            new ConcurrentDictionary<string, Server>();


        public FlightPlan GetFlightPlan(string key)
        {
            if (!flightPlans.ContainsKey(key))
            {
                return null;
            }
            FlightPlan flightPlan = flightPlans[key];
            if (flightPlan == null)
            {
                Console.WriteLine("doesn't exist");
            }
            return flightPlan;
        }

        public string InsertFlightPlan(FlightPlan flightPlan)
        {
            string flightId = makeUniqueId();
            if (!flightPlans.ContainsKey(flightId))
            {
                flightPlans[flightId] = flightPlan;
            }
            else
            {
                flightId = null;
            }

            return flightId;
        }

        // makes 8 characters unique id
        private string makeUniqueId()
        {
            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numbers = "0123456789";
            string id = "";
            for (int i = 0; i < 2; i++)
            {
                id += characters[random.Next(characters.Length)];
            }
            for (int j = 0; j < 6; j++)
            {
                id += numbers[random.Next(numbers.Length)];
            }
            return id;
        }

        public string DeleteFlight(string id)
        {
            if (!flightPlans.ContainsKey(id))
            {
                return "not inside";
            }
            FlightPlan fp = new FlightPlan();

            bool removed = flightPlans.Remove(id, out fp);
            if (removed)
            {
                return "success";
            }
            else
            {
                return "failed to remove";
            }
        }


        public async Task<List<Flight>> GetAllFlights(string dateTime, bool isExternal)
        {
            List<Flight> currentFlights = new List<Flight>();

            // todo add servers if isExternal == true
            if (isExternal)
            {
                List<Flight> fromServers = await GetFlightsFromServers(dateTime);
                foreach (Flight flight in fromServers)
                {
                    flight.IsExternal = true;
                }
                currentFlights.AddRange(fromServers);
            }
            isExternal = false;

            DateTime givenTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(dateTime));

            // goes over all flight plans and checks if flight is active at given time
            // if it's active put the flight with the current location in the list
            foreach (KeyValuePair<string, FlightPlan> idAndPlan in flightPlans)
            {
                string initialTimeToParse = idAndPlan.Value.Location.DateTime;
                DateTime initialTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(initialTimeToParse));
                int comparison = initialTime.CompareTo(givenTime);
                // Or time is at the beginning of flight or it's inside a running flight
                if (comparison == 0)
                {
                    currentFlights.Add(new Flight(idAndPlan.Key, false, idAndPlan.Value));
                }
                else if (comparison < 0)
                {
                    // get flight with the current location andd add it to list of flights
                    Flight flight = await GetFlightWithCurrentLocation(initialTime, givenTime, idAndPlan.Value,
                        isExternal, idAndPlan.Key);
                    if (flight != null)
                    {
                        currentFlights.Add(flight);
                    }
                }
            }
            return currentFlights;
        }

        private async Task<Flight> GetFlightWithCurrentLocation(DateTime initialTime, DateTime givenTime,
            FlightPlan plan, bool isExternal, string id)
        {
            Tuple<double, double> initialLocation =
                new Tuple<double, double>(plan.Location.Longitude, plan.Location.Latitude);
            // go over all segments of flight and checks of it's inside it
            foreach (Segment segment in plan.Segments)
            {
                double seconds = segment.TimeSpanSeconds;
                Tuple<double, double> endLocation = new Tuple<double, double>(segment.Longitude, segment.Latitude);
                DateTime endTime = initialTime.AddSeconds(seconds);
                // if it's inside the segment get the current location according to time
                if (givenTime >= initialTime && givenTime < endTime)
                {
                    Tuple<double, double> currentLocation = await Interpolation(initialLocation, endLocation,
                        initialTime, givenTime, seconds);
                    Flight flight = new Flight(id, isExternal, plan);
                    flight.Longitude = currentLocation.Item1;
                    flight.Latitude = currentLocation.Item2;
                    return flight;
                }
                initialTime = endTime;
                initialLocation = endLocation;
            }
            return null;
        }

        private async Task<Tuple<double, double>> Interpolation(Tuple<double, double> firstLocation,
            Tuple<double, double> secondLocation, DateTime begin, DateTime now, double totalSeconds)
        {
            if (firstLocation.Equals(secondLocation))
            {
                return firstLocation;
            }
            // time from beginning to given time
            TimeSpan difference = now.Subtract(begin);
            // relative time difference
            double relativeDifference = difference.TotalSeconds / totalSeconds;
            // calculate distance between initial location and end of segment
            double distance = Math.Sqrt(Math.Pow((secondLocation.Item2 - firstLocation.Item2), 2)
                + Math.Pow((secondLocation.Item1 - firstLocation.Item1), 2));

            // calculate the wanted longitude and latitude from initial location
            //(according to relative differnece of time)
            double wantedLongitude = firstLocation.Item1 + (secondLocation.Item1 - firstLocation.Item1)
                * relativeDifference;
            double wantedLatitude = firstLocation.Item2 + (secondLocation.Item2 - firstLocation.Item2)
                            * relativeDifference;

            return new Tuple<double, double>(wantedLongitude, wantedLatitude);
        }

        public IEnumerable<Server> GetAllServers()
        {
            return servers.Values.AsEnumerable();
        }

        public string InsertServer(Server server)
        {
            if (!servers.ContainsKey(server.ServerId))
            {
                servers[server.ServerId] = server;
                return "Success";
            }
            return "Already inside";
        }

        public string DeleteServer(string id)
        {
            Server server;
            bool removed = servers.Remove(id, out server);
            if (removed)
            {
                return "Success";
            }
            else
            {
                return "Not Inside";
            }
        }


        private async Task<List<Flight>> GetFlightsFromServers(string relativeTo)
        {
            List<Flight> serversFlights = new List<Flight>();
            foreach (Server server in servers.Values)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.None
                };
                dynamic response = await MakeRequest(server.ServerURL +
                    "/Flights?relative_to=" + relativeTo);
                foreach (var item in response)
                {
                    serversFlights.Add(makeFlightFromJson(item));
                }
            }

            return serversFlights;
        }

        public Flight makeFlightFromJson(JToken flight)
        {
            int passengers = (int)flight["passengers"];
            string flightId = (string)flight["flight_id"];
            double longitude = (double)flight["longitude"];
            double latitude = (double)flight["longitude"];
            string companyName = (string)flight["company_name"];
            string dateTime = (string)flight["date_time"];
            bool isExternal = (bool)flight["isExternal"];

            return new Flight(flightId, longitude, latitude, passengers, companyName, dateTime, isExternal);
        }

        public string HttpGet(string URI)
        {
            WebClient client = new WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream data = client.OpenRead(URI);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();

            return s;
        }


        public static async Task<dynamic> MakeRequest(string url)
        {
            using var client = new HttpClient();
            var result = await client.GetStringAsync(url);
            dynamic json = JsonConvert.DeserializeObject(result);
            return json;
        }


    }
}
