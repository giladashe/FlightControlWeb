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
        // Mocks.
        private readonly Mock<IFlightsManager> managerMock = new Mock<IFlightsManager>();
        private readonly Mock<HttpContext> myContext = new Mock<HttpContext>();

        [Fact]
        public async Task GetAllFlightsGetHttpRequestException_ShouldReturnBadRequest()
        {

            // Arrange
            this.managerMock.Setup(x => x.GetAllFlights(It.IsAny<string>(), It.IsAny<bool>())).Throws(
                new HttpRequestException());
            this.myContext.SetupGet(x => x.Request.QueryString).Returns(new QueryString
                        ("?relative_to=2020-11-27T01:56:22Z"));
            var controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            var controller = new FlightsController(managerMock.Object)
            {
                ControllerContext = controllerContext,
            };

            // Act

            var response = await controller.GetAllFlights("asdfd");


            // Assert check if he handles HttpRequestException

            var action = Assert.IsType<BadRequestObjectResult>(response.Result);
            string message = Assert.IsAssignableFrom<string>(action.Value);
            Assert.Equal("problem in request to servers", message);
        }

        [Fact]
        public async Task GetAllFlightsGetFormatException_ShouldReturnBadRequest()
        {

            // Arrange
            this.managerMock.Setup(x => x.GetAllFlights(It.IsAny<string>(), It.IsAny<bool>())).Throws(
                new FormatException());
            this.myContext.SetupGet(x => x.Request.QueryString).Returns(new QueryString
                        ("?relative_to=2020-11-27T01:56:22Z"));
            var controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            var controller = new FlightsController(managerMock.Object)
            {
                ControllerContext = controllerContext,
            };

            // Act

            ActionResult<IEnumerable<Flight>> response = await controller.GetAllFlights("asdfd");


            // Assert

            var action = Assert.IsType<BadRequestObjectResult>(response.Result);
            string message = Assert.IsAssignableFrom<string>(action.Value);
            Assert.Equal("Date and time not in format", message);
        }

        // Flights to test.
        public static IEnumerable<Flight> GetTestFlights()
        {
            List<Segment> segments = new List<Segment>();
            segments.Add(new Segment(20.3, 10.5, 200));
            segments.Add(new Segment(40.5, 20.5, 500));
            var plan = new FlightPlan(100, "ELAL", new InitialLocation(30.3, 40.5, "2020-11-27T01:56:22Z"), segments);
            var otherSegments = new List<Segment>
            {
                new Segment(20.3, 10.5, 200),
                new Segment(40.5, 20.5, 500)
            };
            var otherPlan = new FlightPlan(100, "SWISSAIR",
                new InitialLocation(30.3, 40.5, "2020-11-27T01:56:22Z"), otherSegments);
            var flights = new List<Flight>();
            flights.Add(new Flight("asdsfda", true, plan));
            flights.Add(new Flight("wow", false, otherPlan));
            return flights.AsEnumerable();
        }
    }
}
