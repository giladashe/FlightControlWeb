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
    public class FlightPlansController : ControllerBase
    {

        private PlansManager manager = new PlansManager();

        // GET: api/FlightPlans
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/FlightPlans/5
        [HttpGet("{id}", Name = "GetFlightPlan")]
        public FlightPlan GetFlightPlan(string id)
        {
            return manager.GetFlightPlan(id);
        }

        // POST: api/FlightPlans
        [HttpPost]
        public void Post([FromBody] FlightPlan plan)
        {
            

            string answer = manager.InsertFlightPlan(new FlightPlan());
            //Console.WriteLine(answer);
        }

        // PUT: api/FlightPlans/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            string answer = manager.DeleteFlight(id);
        }
    }
}
