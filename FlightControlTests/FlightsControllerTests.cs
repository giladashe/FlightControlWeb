using FlightControlWeb.Controllers;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FlightControlTests
{
    public class FlightsControllerTests
    {
        [Fact]
        public async Task GetAllFlights_CatchException()
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
            //IEnumerable<Flight> flights = await GetTestFlights();


            var response = await controller.GetAllFlights("asdfd");


            // check if he handles HttpRequestException

            var action = Assert.IsType<BadRequestObjectResult>(response.Result);
            var message = Assert.IsAssignableFrom<string>(action.Value);
            Assert.Equal("problem in request to servers", message);

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
    }
}
