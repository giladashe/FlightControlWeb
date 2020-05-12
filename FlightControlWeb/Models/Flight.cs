using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Flight
    {
        [JsonProperty("flight_id")]
        public string FlightId { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("passengers")]
        public int Passengers { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("date_time")]
        public string DateTime { get; set; }

        [JsonProperty("is_external")]
        public bool IsExternal { get; set; }

        public Flight(string id, bool isExternal, FlightPlan plan)
        {
            FlightId = id;
            IsExternal = isExternal;
            Longitude = plan.Location.Longitude;
            Latitude = plan.Location.Latitude;
            Passengers = plan.Passengers;
            CompanyName = plan.CompanyName;
            DateTime = plan.Location.DateTime;
        }

    }
}
