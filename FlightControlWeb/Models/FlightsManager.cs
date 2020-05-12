using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightsManager : IFlightsManager
    {
        private static ConcurrentDictionary<string, FlightPlan> flightPlans = new ConcurrentDictionary<string, FlightPlan>();
        private static ConcurrentDictionary<string, Server> servers = new ConcurrentDictionary<string, Server>();


        public FlightPlan GetFlightPlan(string key)
        {
            /* if(key == "5")
             {
                 flightPlans.TryAdd("wow", new FlightPlan(216, "baasfa",new InitialLocation(30.23423, 234.234, "2020asfafas"),
                     new List<Segment>() { new Segment(40.23423, 2234.234, 365)}));
                 return flightPlans["wow"];
             }*/
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
                return "not inside";
            }
        }
        

        public IEnumerable<Flight> GetAllFlights(string dateTime, bool isExternal)
        {

            // todo add servers if isExternal == true

            string timePattern = "yyyy-MM-ddTHH:mm:ssZ";
            DateTime givenTime = DateTime.ParseExact(dateTime,
                    timePattern, System.Globalization.CultureInfo.InvariantCulture);
            List<Flight> currentFlights = new List<Flight>();

            // goes over all flight plans and checks if flight is active at given time
            // if it's active put the flight with the current location in the list
            foreach (KeyValuePair<string, FlightPlan> idAndPlan in flightPlans)
            {
                string initialTimeToParse = idAndPlan.Value.Location.DateTime;
                DateTime initialTime = DateTime.ParseExact(initialTimeToParse,
                    timePattern, System.Globalization.CultureInfo.InvariantCulture);
                int comparison = initialTime.CompareTo(givenTime);
                // Or time is at the beginning of flight or it's inside a running flight
                if (comparison == 0)
                {
                    currentFlights.Add(new Flight(idAndPlan.Key, false, idAndPlan.Value));
                }
                else if (comparison < 0)
                {
                    // get flight with the current location andd add it to list of flights
                    Flight flight = getFlightWithCurrentLocation(initialTime, givenTime, idAndPlan.Value,
                        isExternal, idAndPlan.Key);
                    if (flight != null)
                    {
                        currentFlights.Add(flight);
                    }
                }
            }
            return currentFlights;
        }

        private Flight getFlightWithCurrentLocation(DateTime initialTime, DateTime givenTime,
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
            // time from beginning to given time
            TimeSpan difference = now.Subtract(begin);
            // relative time difference
            double relativeDifference = difference.TotalSeconds / totalSeconds;
            // calculate distance between initial location and end of segment
            double distance = Math.Sqrt(Math.Pow((secondLocation.Item2 - firstLocation.Item2), 2)
                + Math.Pow((secondLocation.Item1 - firstLocation.Item1), 2));

            // calculate the wanted distance from initial location (according to relative 
            // differnece of time)
            double wantedDistance = relativeDifference * distance;

            // calculate Longitude and Latitude according to wanted distance
            double wantedLongitude = firstLocation.Item1
                - (wantedDistance * (firstLocation.Item1 - secondLocation.Item1) / distance);
            if (wantedLongitude < 0)
            {
                wantedLongitude = -wantedLongitude;
            }
            double wantedLatitude = firstLocation.Item2
                - (wantedDistance * (firstLocation.Item2 - secondLocation.Item2) / distance);
            if (wantedLatitude < 0)
            {
                wantedLatitude = -wantedLatitude;
            }
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
    }
}
