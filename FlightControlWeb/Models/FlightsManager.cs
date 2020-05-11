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
            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numbers = "0123456789";
            string flightId = "";
            // Makes a unique ID.
            for (int i = 0; i < 2; i++)
            {
                flightId += characters[random.Next(characters.Length)];
            }
            for (int j = 0; j < 6; j++)
            {
                flightId += numbers[random.Next(numbers.Length)];
            }
            if (!flightPlans.ContainsKey(flightId))
            {
                flightPlans[flightId] = flightPlan;
            }
            else
            {
                flightId = "0";
            }

            return flightId;
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

        public IEnumerable<Flight> GetAllFlights(string dateTime)
        {
            DateTime givenTime = DateTime.ParseExact(dateTime,
                    "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            List<Flight> currentFlights = new List<Flight>();
            foreach (KeyValuePair<string, FlightPlan> idAndPlan in flightPlans)
            {
                string initialTimeToParse = idAndPlan.Value.Location.DateTime;
                DateTime initialTime = DateTime.ParseExact(initialTimeToParse,
                    "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                int comparison = initialTime.CompareTo(givenTime);
                // or it's the beginning of flight or it's inside a running flight
                if (comparison == 0 || (comparison > 0 && CheckIfInside(initialTime,givenTime,idAndPlan.Value)))
                {
                    currentFlights.Add(new Flight(idAndPlan.Key, false, idAndPlan.Value));
                }
            }
            return currentFlights;
        }

        private bool CheckIfInside(DateTime initialTime, DateTime givenTime, FlightPlan plan)
        {
            DateTime endTime = initialTime;
            // go to last segment and check
            foreach (Segment segment in plan.Segments)
            {
                TimeSpan timeSpan = new TimeSpan(segment.TimeSpanSeconds);
                endTime += timeSpan;
            }
            if (givenTime < endTime)
            {
                return true;
            }
            return false;
        }
        public IEnumerable<Flight> GetAllFlightsAllServers(string dateTime)
        {
            return null;
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
