using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    [Serializable]
    public class Server
    {
        [JsonPropertyName("server_id")]
        public string ServerId { get; set; }

        [JsonPropertyName("server_url")]
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
