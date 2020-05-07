using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class InitialLocation
    {
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("date_time")]
        public string DateTime { get; set; }

        [JsonConstructor]
        public InitialLocation(double longitude, double latitude, string dateTime)
        {
            Longitude = longitude;
            Latitude = latitude;
            DateTime = dateTime;
        }

        public InitialLocation()
        {
        }
    }
}
