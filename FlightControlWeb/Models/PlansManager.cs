using FlightControlWeb.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class PlansManager
    {
        private static ConcurrentDictionary<string, FlightPlan> flightPlans = new ConcurrentDictionary<string, FlightPlan>();

        public FlightPlan GetFlightPlan(string key)
        {
            if(key == "5")
            {
                flightPlans.TryAdd("wow", new FlightPlan(216, "baasfa",new InitialLocation(30.23423, 234.234, "2020asfafas"),
                    new List<Segment>() { new Segment(40.23423, 2234.234, 365)}));
                return flightPlans["wow"];
            }
            if (!flightPlans.ContainsKey(key))
            {
                return null;
            }
            FlightPlan flightPlan = flightPlans[key];
            if(flightPlan == null)
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
            if(!flightPlans.ContainsKey(id)){
                return "not inside";
            }
            FlightPlan fp = new FlightPlan();
            
            bool removed = flightPlans.Remove(id,out fp);
            if (removed)
            {
                return "success";
            }
            else
            {
                return "not inside";
            }
        }
    }
}
