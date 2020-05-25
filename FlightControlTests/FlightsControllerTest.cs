using FlightControlWeb.Controllers;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace FlightControlTests
{
    [TestClass]
    public class FlightsControllerTest
    {
        [TestMethod]
        public async Task GetFlightsMultipleServers()
        {
            var managermock = new Mock<IFlightsManager>();
            managermock.Setup(x => x.GetAllFlights(It.IsAny<string>(), It.IsAny<bool>())).Throws(
                new HttpRequestException());
            var myContext = new Mock<HttpContext>();
            myContext.SetupGet(x => x.Request.QueryString).Returns(new QueryString
                        ("?relative_to=2020-11-27T01:56:22Z"));
            var controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            var controller = new FlightsController(managermock.Object)
            {
                ControllerContext = controllerContext,
            };
            IEnumerable<Flight> flights = await GetTestFlights();
            

            var response = controller.GetAllFlights("asdfd");


            // check if he handles HttpRequestException



            /* IEnumerable<Flight> flights1 = await controller.GetAllFlights("?relative_to=2020-11-27T01:56:22Z");*/
            /* Assert.AreEqual(flights.Count(), flights1.Count());
             IEnumerator<Flight> e1 = flights.GetEnumerator();
             IEnumerator<Flight> e2 = flights1.GetEnumerator();
             while (e1.MoveNext() && e2.MoveNext())
             {
                 Assert.AreEqual(e1.Current, e2.Current);
             }
             Assert.AreEqual(flights, flights1);*/

        }

        private Task<IEnumerable<Flight>> GetTestFlights()
        {
            List<Segment> segments = new List<Segment>();
            segments.Add(new Segment(20.3, 10.5, 200));
            segments.Add(new Segment(40.5, 20.5, 500));
            FlightPlan plan = new FlightPlan(100, "ELAL", new InitialLocation(30.3, 40.5, "2020-11-27T01:56:22Z"), segments);
            List<Segment> segments1 = new List<Segment>
            {
                new Segment(20.3, 10.5, 200),
                new Segment(40.5, 20.5, 500)
            };
            FlightPlan plan1 = new FlightPlan(100, "SWISSAIR",
                new InitialLocation(30.3, 40.5, "2020-11-27T01:56:22Z"), segments);
            List<Flight> flights = new List<Flight>();
            flights.Add(new Flight("asdsfda", true, plan));
            flights.Add(new Flight("wow", false, plan1));
            return Task.Run(() => flights.AsEnumerable());
        }


        // checks if handles FormatException
    }
}


/*// GET: api/Flights?relative_to=<DATE_TIME>
[HttpGet]
public async Task<IEnumerable<Flight>> GetAllFlights([FromQuery(Name = "relative_to")]string relativeTo)
{
    string request = Request.QueryString.Value;
    bool isExternal = request.Contains("sync_all");

    return await manager.GetAllFlights(relativeTo, isExternal);
}
*/
