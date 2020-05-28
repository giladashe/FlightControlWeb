using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightsManager : IFlightsManager
    {
        private static ConcurrentDictionary<string, FlightPlan> flightPlans;
        private static ConcurrentDictionary<string, Server> servers;
        //maps from flight plan id to server id
        private static ConcurrentDictionary<string, string> idFromServers;

        public FlightsManager(ConcurrentDictionary<string, FlightPlan> flightPlanDict,
            ConcurrentDictionary<string, Server> serversDict,
            ConcurrentDictionary<string, string> idFromServersDict)
        {
            flightPlans = flightPlanDict;
            servers = serversDict;
            idFromServers = idFromServersDict;
        }


        public async Task<FlightPlan> GetFlightPlan(string key)
        {
            FlightPlan plan = null;
            if (flightPlans.ContainsKey(key))
            {
                plan = flightPlans[key];
            }
            else if (idFromServers.ContainsKey(key))
            {
                Server server = servers[idFromServers[key]];
                plan = await GetFlightPlanFromServer(key, server);
            }
            return plan;
        }

        public string InsertFlightPlan(FlightPlan flightPlan)
        {
            if (!IsValidFlightPlan(flightPlan))
            {
                throw new Exception("Not a valid flight plan");
            }
            // checks if date and tine are in format and throws execption if not
            DateTime.Parse(flightPlan.Location.DateTime);
            string flightId = MakeUniqueId();
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

        // returns true if it's a valid flightplan
        private bool IsValidFlightPlan(FlightPlan flightPlan)
        {
            if (flightPlan == null || flightPlan.CompanyName == null
                || flightPlan.Location == null || flightPlan.Location.DateTime == null ||
                flightPlan.Segments == null ||
                !IsValidLonLat(flightPlan.Location.Longitude, flightPlan.Location.Latitude))
            {
                return false;
            }
            foreach (Segment segment in flightPlan.Segments)
            {
                if (!IsValidLonLat(segment.Longitude, segment.Latitude) ||
                    segment.TimeSpanSeconds <= 0)
                {
                    return false;
                }
            }
            return true;
        }


        // makes 8 characters unique id
        private string MakeUniqueId()
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
            FlightPlan plan = new FlightPlan();

            bool removed = flightPlans.Remove(id, out plan);
            if (removed)
            {
                return "success";
            }
            else
            {
                return "failed to remove";
            }
        }


        public async Task<IEnumerable<Flight>> GetAllFlights(string dateTime, bool isExternal)
        {

            List<Flight> currentFlights = new List<Flight>();
            // get flights from server if is external
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
                Flight newFlight = AddFlightFromThisServer(idAndPlan, givenTime, isExternal);
                //it's a relevant flight for the time
                if (newFlight != null)
                {
                    currentFlights.Add(newFlight);
                }
            }
            return currentFlights;
        }

        private Flight AddFlightFromThisServer(KeyValuePair<string, FlightPlan> idAndPlan, DateTime givenTime, bool isExternal)
        {
            string initialTimeToParse = idAndPlan.Value.Location.DateTime;
            DateTime initialTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(initialTimeToParse));
            int comparison = initialTime.CompareTo(givenTime);
            // Or time is at the beginning of flight or it's inside a running flight
            if (comparison == 0)
            {
                return new Flight(idAndPlan.Key, false, idAndPlan.Value);
            }
            else if (comparison < 0)
            {
                // get flight with the current location
                Flight flight = GetFlightWithCurrentLocation(initialTime, givenTime, idAndPlan.Value,
                    isExternal, idAndPlan.Key);
                if (flight != null)
                {
                    return flight;
                }
            }
            return null;
        }


        private Flight GetFlightWithCurrentLocation(DateTime initialTime, DateTime givenTime,
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
                    Tuple<double, double> currentLocation = Interpolation(initialLocation, endLocation,
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

        private Tuple<double, double> Interpolation(Tuple<double, double> firstLocation,
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
            Server server = new Server();
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

        private async Task<FlightPlan> GetFlightPlanFromServer(string planId, Server server)
        {
            FlightPlan flightPlan = null;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            };

            dynamic response = await MakeRequest(server.ServerURL +
                "/api/FlightPlan/" + planId);
            if (response != null)
            {
                flightPlan = MakeFlightPlanFromJson(response);
            }
            return flightPlan;
        }

        private async Task<List<Flight>> GetFlightsFromServers(string relativeTo)
        {
            List<Flight> serversFlights = new List<Flight>();
            List<Flight> flightsFromServer = new List<Flight>();
            foreach (Server server in servers.Values)
            {
                try
                {
                    flightsFromServer = await GetFlightsFromServer(relativeTo, server);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (flightsFromServer != null && flightsFromServer.Count > 0)
                    {
                        serversFlights.AddRange(flightsFromServer);
                        flightsFromServer.Clear();
                    }
                }
            }
            return serversFlights;
        }


        private async Task<List<Flight>> GetFlightsFromServer(string relativeTo, Server server)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            };
            List<Flight> flights = new List<Flight>();
            try
            {
                dynamic response = await MakeRequest(server.ServerURL +
                "/api/Flights?relative_to=" + relativeTo);

                if (response == null)
                {
                    return flights;
                }
                string responseStr = response.ToString();
                //doesn't have any flights
                if (!responseStr.Contains("flight_id"))
                {
                    return flights;
                }

                foreach (var item in response)
                {
                    AddNewFlightFromOtherServers(flights, item, server.ServerId);
                }
                return flights;
            }
            catch (Exception)
            {
                return null;
            }
        }


        private void AddNewFlightFromOtherServers(List<Flight> flights, JToken jsonFlight,
            string serverId)
        {
            Flight newFlight = MakeFlightFromJson(jsonFlight);
            if (newFlight != null && !flightPlans.ContainsKey(newFlight.FlightId))
            {
                flights.Add(newFlight);
                if (!idFromServers.ContainsKey(newFlight.FlightId))
                {
                    idFromServers.TryAdd(newFlight.FlightId, serverId);
                }
            }
        }


        private Flight MakeFlightFromJson(JToken flight)
        {
            if (flight == null)
            {
                return null;
            }
            try
            {
                int passengers = (int)flight["passengers"];
                string flightId = (string)flight["flight_id"];
                double longitude = (double)flight["longitude"];
                double latitude = (double)flight["latitude"];
                string companyName = (string)flight["company_name"];
                string dateTime = (string)flight["date_time"];
                bool isExternal = (bool)flight["is_external"];
                // if it's not a valid date and time throws exception

                if (flightId == null || companyName == null || dateTime == null ||
                   !IsValidLonLat(longitude, latitude))
                {
                    return null;
                }
                // throws exception if not a valid date time
                DateTime.Parse(dateTime);
                return new Flight(flightId, longitude, latitude, passengers,
                    companyName, dateTime, isExternal);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsValidLonLat(double longitude, double latitude)
        {
            //latitude values (Y-values) range between -90 and +90 degrees.
            return longitude >= -180 && longitude <= 180 && latitude >= -90 && latitude <= 90;
        }
        private FlightPlan MakeFlightPlanFromJson(JToken flightPlan)
        {
            int passengers = (int)flightPlan["passengers"];
            string companyName = (string)flightPlan["company_name"];
            double longitude = (double)flightPlan["initial_location"]["longitude"];
            double latitude = (double)flightPlan["initial_location"]["latitude"];
            string dateTime = (string)flightPlan["initial_location"]["date_time"];
            // if it's not a valid date and time throws exception
            DateTime.Parse(dateTime);
            InitialLocation location = new InitialLocation(longitude, latitude, dateTime);
            JArray jsonSegments = (JArray)flightPlan["segments"];
            List<Segment> segments = new List<Segment>();
            foreach (var segment in jsonSegments)
            {
                double longitudeSegment = (double)segment["longitude"];
                double latitudeSegment = (double)segment["latitude"];
                double timeSpan = (double)segment["timespan_seconds"];
                if (!IsValidLonLat(longitudeSegment, latitudeSegment) || timeSpan <= 0)
                {
                    return null;
                }
                Segment newSegment = new Segment(longitudeSegment, latitudeSegment, timeSpan);
                segments.Add(newSegment);
            }

            return new FlightPlan(passengers, companyName, location, segments);
        }

        private async Task<dynamic> MakeRequest(string url)
        {
            using var client = new HttpClient();
            //check if throws timeout exception
            client.Timeout = TimeSpan.FromSeconds(20);
            var result = await client.GetStringAsync(url);
            dynamic json = JsonConvert.DeserializeObject(result);
            return json;
        }
    }
}
