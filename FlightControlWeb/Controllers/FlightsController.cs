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
    public class FlightsController : ControllerBase
    {
        private IFlightsManager manager;

        public FlightsController(IFlightsManager manager)
        {
            this.manager = manager;
        }

        // GET: api/Flights?relative_to=<DATE_TIME>
        [HttpGet]
        public IEnumerable<Flight> GetAllFlightsFromServer(string relative_to)
        {
            string request = Request.QueryString.Value;
            bool isExternal = request.Contains("sync_all");
            
            return manager.GetAllFlights(relative_to,isExternal);
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
