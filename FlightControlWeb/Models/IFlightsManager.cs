using FlightControlWeb.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace FlightControlWeb.Models
{
    public interface IFlightsManager
    {

        FlightPlan GetFlightPlan(string key);

        string InsertFlightPlan(FlightPlan flightPlan);

        string DeleteFlight(string id);

        IEnumerable<Flight> GetAllFlights(string dateTime, bool isExternal);

        IEnumerable<Server> GetAllServers();

        string InsertServer(Server server);

        string DeleteServer(string id);

    }
}
