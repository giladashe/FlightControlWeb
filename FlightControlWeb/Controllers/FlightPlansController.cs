﻿using System;
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

        private IFlightsManager manager;

        public FlightPlansController(IFlightsManager manager)
        {
            this.manager = manager;
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
            

            string answer = manager.InsertFlightPlan(plan);
            Console.WriteLine(answer);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            string answer = manager.DeleteFlight(id);
        }
    }
}
