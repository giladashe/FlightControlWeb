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

        private readonly IFlightsManager manager;

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
                return BadRequest("Problem with http request to other servers");
            }
            catch (Exception e)
            {
                if (e.Message == "The operation was canceled.")
                {
                    return BadRequest("Timeout - server didn't bring the flight plan");
                }
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
            try
            {
                string id = manager.InsertFlightPlan(plan);
                if (id != null)
                {
                    return Ok(id);
                }
                else
                {
                    // had id in the dictionary already
                    return BadRequest("Had id in the dictionary already");
                }
            }
            catch (FormatException)
            {
                return BadRequest("Date and time not in format");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
