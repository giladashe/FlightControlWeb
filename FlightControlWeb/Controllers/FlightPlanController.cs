using System;
using System.Net.Http;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {

        private IFlightsManager manager;

        public FlightPlanController(IFlightsManager manager)
        {
            this.manager = manager;
        }

        // GET: api/FlightPlans/5
        [HttpGet("{id}", Name = "GetFlightPlan")]
        public async Task<ActionResult<FlightPlan>> GetFlightPlan(string id)
        {
            FlightPlan plan;
            try
            {
                plan = await manager.GetFlightPlan(id);
            }
            catch (HttpRequestException)
            {
                return BadRequest("Problem with http request to other servers\n");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            if (plan == null)
            {
                return NotFound();
            }
            return Ok(plan);
        }

        // POST: api/FlightPlans
        [HttpPost]
        public ActionResult<string> AddFlightPlan([FromBody] FlightPlan plan)
        {
            string answer = manager.InsertFlightPlan(plan);
            if (answer != null)
            {
                return Ok("success");
            }
            else
            {
                // had id in the dictionary already
                return BadRequest();
            }

        }
    }
}
