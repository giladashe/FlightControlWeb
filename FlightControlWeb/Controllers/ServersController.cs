using System;
using System.Collections.Generic;
using System.Linq;
using FlightControlWeb.Models;
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
        public ActionResult<IEnumerable<Server>> GetAllServers()
        {
            IEnumerable<Server> servers = manager.GetAllServers();
            if (servers.Any())
            {
                return Ok(servers);
            }
            return NotFound();
        }

        // POST: api/Servers
        [HttpPost]
        public ActionResult<string> InsertNewServer([FromBody] Server server)
        {
            string url = server.ServerURL;
            if (url.EndsWith('/'))
            {
                server.ServerURL = url.Remove(url.Length-1);
            }
            string response = manager.InsertServer(server);
            if(response == "success")
            {
                return Ok(response);
            }
            // already inside
            return BadRequest(response);
        }


        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public ActionResult<string> DeleteServer(string id)
        {
            string response = manager.DeleteServer(id);
            if (response == "success")
            {
                return Ok(response);
            }
            // not inside
            return BadRequest(response);
        }
    }
}
