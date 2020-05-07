using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Segment
    {
        [JsonPropertyName ("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("timespan_seconds")]
        public double TimeSpanSeconds { get; set; }

        [JsonConstructor]
        public Segment(double longitude, double latitude, double timeSpanSeconds)
        {
            Longitude = longitude;
            Latitude = latitude;
            TimeSpanSeconds = timeSpanSeconds;
        }

        public Segment()
        {
        }

        /*public Segment(double longitude,double latitude,double timeSpanSeconds)
        {
            TargetLocation = new Location(longitude, latitude);
            TimeSpanSeconds = timeSpanSeconds;
        }

        public Segment(Location targetLocation, double timeSpanSeconds)
        {
            TargetLocation = targetLocation;
            TimeSpanSeconds = timeSpanSeconds;
        }*/

    }
}
