using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private IFlightsManager manager;

        public ServersController(IFlightsManager manager)
        {
            this.manager = manager;
        }


        // GET: api/Servers
        [HttpGet]
        public IEnumerable<Server> GetAllServers()
        {
            return manager.GetAllServers();
        }

        // POST: api/Servers
        [HttpPost]
        public void InsertNewServer([FromBody] Server server)
        {
            string response = manager.InsertServer(server);
        }


        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void DeleteServer(string id)
        {
            string response = manager.DeleteServer(id);
        }
    }
}
