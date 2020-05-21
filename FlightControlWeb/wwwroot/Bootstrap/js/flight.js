//setInterval(getFlights(), 2000);
getFlights();

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
                    + "<td><a href='#'><i class= 'fa fa-trash'></i ></a>" + "</td></tr>");
            } else {
                $("#externalFlightsBody").append("<tr onclick='showFlight(this)'><td>" + flight.flight_id
                    + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.date_time + "</td></tr>");
            }
            showPlaneIcon(flight.latitude, flight.longitude);
        })
    });
}

function showPlaneIcon(lat, lon) {
    
    //let iconBase = "https://maps.google.com/mapfiles/kml/shapes/";
    //let iconPlane = '\Bootstrap\js\aircraft.png';
    let position = new google.maps.LatLng(lat, lon);
    let marker = new google.maps.Marker({
        position: position,
        map: map,
        icon: 'Bootstrap/js/plane.png'
    });
}

//<a href='#'><i class= 'fas fa-trash-alt delete_icon'></i ></a> 
function showFlight() {
    // remove green background of "table-success" from all internalFlights table and add it only to the selected row 
    $("#internalFlights tr").removeClass('table-success');
    let row = event.target.parentNode;
    row.classList.add('table-success');

    // remove table flightPlanBody so the flight appear only once  
    $("#flightDetailsBody tr").empty();

    // fill flightPlan table 
    let flightID = row.children[0].innerText;
    let flightPlanAsk = "/api/FlightPlan/" + flightID;
    $.getJSON(flightPlanAsk, function (flightPlan) {
        // paint flight lines on map 
        paintFlightPath(flightPlan);
        $("#flightDetailsBody").append("<tr><td>" + flightID + "</td><td>" + flightPlan.company_name + "</td><td>" +
            flightPlan.passengers + "</td><td>" + "longitude: " + flightPlan.initial_location.longitude + " &emsp;"
            + "latitude: " + flightPlan.initial_location.latitude + "</td></tr>");
    });

    function paintFlightPath(flightPlan) {
        let lines = [{ lat: Number(flightPlan.initial_location.latitude), lng: Number(flightPlan.initial_location.longitude) }];
        for (let i = 0; i < flightPlan.segments.length; i++) {
            let line = { lat: Number(flightPlan.segments[i].latitude), lng: Number(flightPlan.segments[i].longitude) };
            lines.push(line);
        }
        let flightPath = new google.maps.Polyline({
            path: lines,
            geodesic: true,
            strokeColor: '#4cff00',
            strokeOpacity: 1.0,
            strokeWeight: 2
        });
        flightPath.setMap(map);
    }
}


