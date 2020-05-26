using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FlightControlTests
{
    class FlightsManagerTests
    {

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


            // Assert check if he handles HttpRequestException

            BadRequestObjectResult action = Assert.IsType<BadRequestObjectResult>(response.Result);
            string message = Assert.IsAssignableFrom<string>(action.Value);
            Assert.Equal("problem in request to servers", message);
        }

        /*public async Task<IEnumerable<Flight>> GetAllFlights(string dateTime, bool isExternal)
        {

            List<Flight> currentFlights = new List<Flight>();
            // get flights from server if is external
            if (isExternal)
            {
                List<Flight> fromServers = await GetFlightsFromServers(dateTime);
                foreach (Flight flight in fromServers)
                {
                    flight.IsExternal = true;
                }
                currentFlights.AddRange(fromServers);
            }
            isExternal = false;

            DateTime givenTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(dateTime));

            // goes over all flight plans and checks if flight is active at given time
            // if it's active put the flight with the current location in the list
            foreach (KeyValuePair<string, FlightPlan> idAndPlan in flightPlans)
            {
                Flight newFlight = AddFlightFromThisServer(idAndPlan, givenTime, isExternal);
                if (newFlight != null)
                {
                    currentFlights.Add(newFlight);
                }
            }
            return currentFlights;
        }*/
    }
}
