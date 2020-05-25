using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FlightControlWeb.Models;
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
        
        public async Task<ActionResult<IEnumerable<Flight>>> GetAllFlights([FromQuery(Name = "relative_to")]string relativeTo)
        {
            string request = Request.QueryString.Value;
            bool isExternal = request.Contains("sync_all");
            IEnumerable<Flight> flights = null;
            try
            {
                flights = await manager.GetAllFlights(relativeTo, isExternal);
            }
            catch (HttpRequestException)
            {
                return BadRequest("problem in request to servers");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(flights);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public ActionResult<string> DeleteFlight(string id)
        {
            string answer = manager.DeleteFlight(id);
            if (answer == "success")
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
