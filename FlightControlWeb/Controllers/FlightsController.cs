using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private IFlightsManager manager;

        public FlightsController(IFlightsManager manager)
        {
            this.manager = manager;
        }

        // GET: api/Flights?relative_to=<DATE_TIME>
        [HttpGet]
        public async Task<List<Flight>> GetAllFlights(string relative_to)
        {
            string request = Request.QueryString.Value;
            bool isExternal = request.Contains("sync_all");

            List<Flight> thisFllights = await manager.GetAllFlights(relative_to, isExternal);

            return thisFllights;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public string DeleteFlight(string id)
        {
            string answer = manager.DeleteFlight(id);
            return answer;
        }
    }
}
