//getFlights();
setInterval(getFlights(), 2000);


function getFlights() { 
    let currentTime = new Date().toISOString().substr(0, 19);
    let timeFormat = currentTime + 'Z';
    //let ask = "/api/Flights?relative_to=" + timeFormat + "&sync_all";  
    let ask = "/api/Flights?relative_to=2020-11-27T01:56:21Z&sync_all";
    $.getJSON(ask, function (data) {
        data.forEach(function (flight) {
            //let flightID = flight.flight_id;
            if (!flight.is_external) {
                $("#internalFlightsBody").append("<tr onclick='showFlight(this)' ><td>" + flight.flight_id
                    + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.date_time + "</td>"
                    + "<td><a href='#'><i class= 'fas fa-trash-alt delete_icon'></i ></a>" + "</td></tr>");
            } else {
                $("#externalFlightsBody").append("<tr><td>" + flight.flight_id
                    + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.date_time + "</td>"
                    + "<td><a href='#'><i class= 'fas fa-trash-alt delete_icon'></i ></a>" + "</td></tr>");
            }
        })
    });
}

function showFlight() {
    // remove green background of "table-success" from all internalFlights table and add it only to the selected row 
    $("#internalFlights tr").removeClass('table-success');
    let row = event.target.parentNode;
    row.classList.add('table-success');

    // remove table flightPlanBody so the flight appear only once  
    //document.getElementById("flightDetailsBody").empty();
    $("#flightDetailsBody tr").empty();

    // fill flightPlan table 
    let flightID = row.children[0].innerText;
    let flightPlanAsk = "/api/FlightPlan/" + flightID;
    $.getJSON(flightPlanAsk, function (flightPlan) {
        $("#flightDetailsBody").append("<tr><td>" + flightID + "</td><td>" + flightPlan.company_name + "</td><td>" +
            flightPlan.passengers + "</td><td>" + "longitude: " + flightPlan.initial_location.longitude + " &emsp;"
            + "latitude: " + flightPlan.initial_location.latitude + "</td></tr>");
    });
}

/*function appendItem(data) {
    let internalFlights = document.getElementById("internalFlightsBody");
    let externalFlights = document.getElementById("externalFlightsBody");
    let newFlight = document.createElement('tr');
    let flightID = document.createElement('td');
    let flightCompany = document.createElement('td');

    if (data['is_external'] == false) {
        flightID.textContent = data['flight_id'];
        flightCompany.textContent = data['company_name'];
        newFlight.appendChild(flightID);
        newFlight.appendChild(flightCompany);
        internalFlights.appendChild(newFlight);
    }
}*/

/*fetch(ask)
    .then(response => response.json())
    .then(data => appendItem(data))
    .catch(error => console.log(error));
    //.then(data => console.log(data)); */

