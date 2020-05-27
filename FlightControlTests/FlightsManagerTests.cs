using FlightControlWeb.Controllers;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FlightControlTests
{
    public class FlightsManagerTests
    {
        private readonly Mock<ConcurrentDictionary<string, FlightPlan>> flightPlanDictStub =
            new Mock<ConcurrentDictionary<string, FlightPlan>>();
        private readonly Mock<ConcurrentDictionary<string, Server>> serversDictStub =
            new Mock<ConcurrentDictionary<string, Server>>();
        private readonly Mock<ConcurrentDictionary<string, string>> idFromServersDictStub =
            new Mock<ConcurrentDictionary<string, string>>();
        [Fact]
        public async Task GetAllFlightsWithUnformattedDateShouldRaiseFormatExeception()
        {


            FlightsManager manager =
                new FlightsManager(this.flightPlanDictStub.Object, this.serversDictStub.Object,
                this.idFromServersDictStub.Object);


            var exceptionThrown = false;

            // Act
            try
            {
                IEnumerable<Flight> response = await manager.GetAllFlights("2020-11aef-27T01:56:22Z", false);
            }
            catch (FormatException)
            {

                exceptionThrown = true;
            }

            // Check if Format Exception has been thrown
            Assert.True(exceptionThrown);


        }

        [Fact]
        public async Task GetAllFlightsWithoutSyncAll()
        {
            // Arrange
            List<Flight> flightsExpected = (List<Flight>) FlightsControllerTests.GetTestFlights();
            
            FlightsController controller = ArrangeForSyncTests(false, flightsExpected);

            // Act

            ActionResult<IEnumerable<Flight>> response = 
                await controller.GetAllFlights("2020-11-27T01:56:22Z");

            // Assert

            // Check if response is ok and lists are equal
            Assert.IsType<OkObjectResult>(response.Result);
            OkObjectResult result = (OkObjectResult)response.Result;
            List<Flight> flightsResult = (List<Flight>)result.Value;

            CheckIfListsAreEqual(flightsExpected, flightsResult);

        }

        [Fact]
        public async Task GetAllFlightsWithSyncAll()
        {
            // Arrange
            List<Flight> flightsWithSyncExpected = (List<Flight>)FlightsControllerTests.GetTestFlights();
            flightsWithSyncExpected.Add(new Flight("asf", 1243.12, 1242.12, 200,
                "ELAL", "2020-11-27T01:56:22Z", true));
            // true means that you have sync all
            FlightsController controller = ArrangeForSyncTests(true, flightsWithSyncExpected);

            // Act

            ActionResult<IEnumerable<Flight>> response = await controller.GetAllFlights("2020-11-27T01:56:22Z");

            // Assert

            // Check if response is ok and lists are equal
            Assert.IsType<OkObjectResult>(response.Result);
            OkObjectResult result = (OkObjectResult)response.Result;
            List<Flight> flightsResult = (List<Flight>)result.Value;

            CheckIfListsAreEqual(flightsWithSyncExpected, flightsResult);
        }

        internal static void CheckIfListsAreEqual(List<Flight> expected, List<Flight> returned)
        {
            Assert.Equal(expected.Count(), returned.Count());
            IEnumerator<Flight> e1 = expected.GetEnumerator();
            IEnumerator<Flight> e2 = returned.GetEnumerator();
            //cheks if all the flights are identical
            while (e1.MoveNext() && e2.MoveNext())
            {
                Assert.Equal(e1.Current, e2.Current);
            }
        }


        private FlightsController ArrangeForSyncTests(bool withSync, List<Flight> flightsToReturn)
        {
            Mock<IFlightsManager> managerMock =
                new Mock<IFlightsManager>();
            managerMock.Setup(x => x.GetAllFlights(It.IsAny<string>(), false)).Returns(
                Task.Run(() => flightsToReturn.AsEnumerable()));
            managerMock.Setup(x => x.GetAllFlights(It.IsAny<string>(), true)).Returns(
               Task.Run(() => flightsToReturn.AsEnumerable()));
            Mock<HttpContext> myContext = new Mock<HttpContext>();
            string returnedValue = "?relative_to=2020-11-27T01:56:22Z";
            if (withSync)
            {
                returnedValue = "?relative_to=2020-11-27T01:56:22Z&sync_all";
            }
            myContext.SetupGet(x => x.Request.QueryString).Returns(new QueryString
                (returnedValue));
            ControllerContext controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            FlightsController controller = new FlightsController(managerMock.Object)
            {
                ControllerContext = controllerContext,
            };
            return controller;
        }
    }
}
