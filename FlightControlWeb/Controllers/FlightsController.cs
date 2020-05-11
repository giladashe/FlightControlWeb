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
        [HttpGet("{request}", Name = "GetAllFlightsFromServer")]
        public IEnumerable<Flight> GetAllFlightsFromServer(HttpRequest request)
        {
            return null;
        }

        /*// GET: api/Flights/5
        [HttpGet("{id}","{}", Name = "GetFlightsAllServers")]
        public IEnumerable<Flight> GetFlights(string date)
        {

            return null;
        }*/

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public string Delete(string id)
        {
            string answer = manager.DeleteFlight(id);
            return answer;
        }
    }
}
