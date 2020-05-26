using FlightControlWeb.Controllers;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FlightControlTests
{
    public class FlightsControllerTests
    {
        private Mock<IFlightsManager> managermock = new Mock<IFlightsManager>();
        private Mock<HttpContext> myContext = new Mock<HttpContext>();

        [Fact]
        public async Task GetAllFlightsGetHttpRequestExceptionShouldReturnBadRequest()
        {

            // Arrange
            this.managermock.Setup(x => x.GetAllFlights(It.IsAny<string>(), It.IsAny<bool>())).Throws(
                new HttpRequestException());
            this.myContext.SetupGet(x => x.Request.QueryString).Returns(new QueryString
                        ("?relative_to=2020-11-27T01:56:22Z"));
            ControllerContext controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            FlightsController controller = new FlightsController(managermock.Object)
            {
                ControllerContext = controllerContext,
            };

            // Act

            ActionResult<IEnumerable<Flight>> response = await controller.GetAllFlights("asdfd");


            // Assert

            BadRequestObjectResult action = Assert.IsType<BadRequestObjectResult>(response.Result);
            string message = Assert.IsAssignableFrom<string>(action.Value);
            Assert.Equal("problem in request to servers", message);
        }
        [Fact]
        public async Task GetAllFlightsGetFormatExceptionShouldReturnBadRequest()
        {

            // Arrange
            this.managermock.Setup(x => x.GetAllFlights(It.IsAny<string>(), It.IsAny<bool>())).Throws(
                new FormatException());
            this.myContext.SetupGet(x => x.Request.QueryString).Returns(new QueryString
                        ("?relative_to=2020-11-27T01:56:22Z"));
            ControllerContext controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            FlightsController controller = new FlightsController(managermock.Object)
            {
                ControllerContext = controllerContext,
            };

            // Act

            ActionResult<IEnumerable<Flight>> response = await controller.GetAllFlights("asdfd");


            // Assert

            BadRequestObjectResult action = Assert.IsType<BadRequestObjectResult>(response.Result);
            string message = Assert.IsAssignableFrom<string>(action.Value);
            Assert.Equal("Date and time not in format", message);
        }

        [Fact]
        public async Task GetAllFlightsGetFlightsUnmodified()
        {

            // Arrange
            IEnumerable<Flight> flights = await GetTestFlights();
            this.managermock.Setup(x => x.GetAllFlights(It.IsAny<string>(), It.IsAny<bool>())).Returns(
                Task.Run(() => flights.AsEnumerable()));
            this.myContext.SetupGet(x => x.Request.QueryString).Returns(new QueryString
                        ("?relative_to=2020-11-27T01:56:22Z"));
            ControllerContext controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            FlightsController controller = new FlightsController(managermock.Object)
            {
                ControllerContext = controllerContext,
            };


            // Act

            ActionResult<IEnumerable<Flight>> response = 
                await controller.GetAllFlights("?relative_to=2020-11-27T01:56:22Z");


            // Assert check if he handles HttpRequestException

            var action = Assert.IsType<OkObjectResult>(response.Result);
            OkObjectResult result = (OkObjectResult)response.Result;
            IEnumerable<Flight> flights1 = (IEnumerable<Flight>)result.Value;
            //checks if the flights expected count equals to result
            Assert.Equal(flights.Count(), flights1.Count());
            IEnumerator<Flight> e1 = flights.GetEnumerator();
            IEnumerator<Flight> e2 = flights1.GetEnumerator();
            //cheks if all the flights are identical
            while (e1.MoveNext() && e2.MoveNext())
            {
                Assert.Equal(e1.Current, e2.Current);
            }
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
    }
}
