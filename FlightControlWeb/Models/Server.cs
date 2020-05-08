using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Server
    {
        [JsonProperty("ServerId")]
        public string ServerId { get; set; }

        [JsonProperty("ServerURL")]
        public string ServerURL { get; set; }
        
        [JsonConstructor]
        public Server(string serverId, string serverURL)
        {
            ServerId = serverId;
            ServerURL = serverURL;
        }
        
        public Server() { }
    }
}
