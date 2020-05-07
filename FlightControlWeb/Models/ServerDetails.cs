using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class ServerDetails
    {
        public string ServerId { get; set; }

        public string ServerURL { get; set; }

        public ServerDetails(string serverId, string serverURL)
        {
            ServerId = serverId;
            ServerURL = serverURL;
        }
    }
}
