using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Model;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private PlansManager manager = new PlansManager();
        // GET: api/Flights
        [HttpGet]
        public IEnumerable<string> GetAllFlights()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Flights/5
        [HttpGet("{id}", Name = "GetFlights")]
        public IEnumerable<Flight> GetFlights(string date)
        {

            return null;
        }

        // POST: api/Flights
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Flights/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public string Delete(string id)
        {
            string answer = manager.DeleteFlight(id);
            return answer;
        }
    }
}
