
//variables
var markers = new Array();
var flightPath = { flightId: null, polyLine: null }
let flightsIdsSet = new Set();

//configurations:
//add google map a listener for click():
map.addListener('click', function () {
    //when clicking on map, remove all colored paths or table marks.
    $("#internalFlights tr").removeClass('table-success');
    $("#flightDetailsBody tr").empty();
    flightPath.polyLine.setMap(null);
})


//at beggining, get all flights
getFlights();

function removeMarkers() {
    markers.forEach(function (marker) {
        marker.setMap(null);
    });
    while (markers.length > 0) {
        markers.pop();
    }
}


var intervalId;
$(document).ready(function () {
    intervalId = setInterval(function () {
        movePlanes();
        getFlights();
    }, 2000);
});

//At some other point
clearInterval(intervalId);


function movePlanes() {
    let currentTime = new Date().toISOString().substr(0, 19);
    let timeFormat = currentTime + 'Z';
    let ask = "/api/Flights?relative_to=" + timeFormat + "&sync_all";
    $.getJSON(ask, function (data) {
        removeMarkers();
        data.forEach(function (flight) {
            showPlaneIcon(flight.latitude, flight.longitude, flight.flight_id);
        })
    })
    .fail(function (jqXHR) {
        toastr.error(jqXHR.statusText + ' : ' + jqXHR.responseText);
       })
;
}

function getFlights() {

    let currentTime = new Date().toISOString().substr(0, 19);
    let timeFormat = currentTime + 'Z';
    let ask = "/api/Flights?relative_to=" + timeFormat + "&sync_all";
    flightsIdsSet.clear();
    /*let isPolygonFlighActive = false;*/
    $.getJSON(ask, function (data) {
        //if there is no data to update, so empty the table.        
        if (data.length === 0) { //todo: I'M not sure I want this patch to be here
            $('#internalFlightsBody').empty();
            $('#externalFlightsBody').empty();
        }
        data.forEach(function (flight) {
            flightsIdsSet.add(flight.flight_id);
            if (document.getElementById(flight.flight_id) === null) {
                if (!flight.is_external) {
                    var row = "<tr id=" + flight.flight_id + " onclick=showFlight('" + flight.flight_id + "') ><td>" + flight.flight_id
                        + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.date_time + "</td>"
                        + "<td><a href='#'><i class='fa fa-trash' onclick=deleteFlight(\"" + flight.flight_id + "\")></i ></a>" + "</td></tr>";
                    $("#internalFlightsBody").append(row);
                } else {
                    let externalRow = "<tr id=" + flight.flight_id + " onclick=showFlight('" + flight.flight_id + "') ><td>" + flight.flight_id
                        + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.date_time + "</td>";
                    $("#externalFlightsBody").append(externalRow);                    
                }
            }
        })
        removeInactiveFlights();
    });
}

function removeInactiveFlights() {
    //check internal flights table
    $('#internalFlights tr').each(function () {

        //SECOND: remove polyline
        if (flightsIdsSet.has(this.id) === false &&
            flightPath.flightId === this.id && flightPath.polyLine !== null) {
            flightPath.polyLine.setMap(null);
        }

        //THIRD: remove from table
        if (flightsIdsSet.has(this.id) === false && document.getElementById(this.id) !== null) {
            document.getElementById(this.id).remove();
            //if inactive flight (ended flight) was selected so remove it's polygon and details
            if (document.getElementById("details_" + this.id) !== null) {
                document.getElementById("details_" + this.id).remove();
            }
        }
    });

    $('#externalFlights tr').each(function () {

        //FIRST: if inactive flight (ended flight) was selected so remove it's polygon and details
        if (document.getElementById("details_" + this.id) !== null) {
            document.getElementById("details_" + this.id).remove();
        }

        //SECOND: remove polyline (if exist)
        if (flightsIdsSet.has(this.id) === false &&
            flightPath.flightId === this.id && flightPath.polyLine !== null) {
            flightPath.polyLine.setMap(null);
        }

        //THIRD: remove from table
        if (flightsIdsSet.has(this.id) === false && document.getElementById(this.id) !== null) {
            document.getElementById(this.id).remove();
            //if inactive flight (ended flight) was selected so remove it's polygon and details
            if (document.getElementById("details_" + this.id) !== null) {
                document.getElementById("details_" + this.id).remove();
            }
        }
    });

}

function isExist(flight_id) {
    //let exist = $("#internalFlights tr")
    let id = '#' + flight_id;
    //alert(id);
    if ($(id).length) {
        return true;
    }
    return false;
}



function showPlaneIcon(lat, lon, flightID) {

    //let iconBase = "https://maps.google.com/mapfiles/kml/shapes/";
    //let iconPlane = '\Bootstrap\js\aircraft.png';  
    let position = new google.maps.LatLng(lat, lon);
    let marker = new google.maps.Marker({
        map: map,
        position: position,
        icon: 'Bootstrap/js/plane.png'
    });

    marker.addListener('click', function () {
        map.setCenter(marker.getPosition());
        smoothZoom(map, 8, map.getZoom());
        showFlight(flightID);

        //


        //let flightPlanAsk = "/api/FlightPlan/" + flightID;
        //$.getJSON(flightPlanAsk, function (flightPlan) {
        //    paintFlightPath(flightPlan, flightID);
        //});

    })
    markers.push(marker);
}



function showFlight(flightID) {


    // remove green background of "table-success" from all internalFlights table and add it only to the selected row
    $("#internalFlights tr").removeClass('table-success');

    $("#externalFlights tr").removeClass('table-success');

    //let row = event.target.parentNode;
    $('#' + flightID).addClass('table-success');

    
    // remove table flightPlanBody so the flight appear only once  
    $("#flightDetailsBody tr").empty();

    // fill flightPlan table 
    //let flightID = row.children[0].innerText;
    let flightPlanAsk = "/api/FlightPlan/" + flightID;
    $.getJSON(flightPlanAsk, function (flightPlan) {
        // paint flight lines on map
        paintFlightPath(flightPlan, flightID);
        //id=" + flightID + "
        $("#flightDetailsBody").append("<tr id=details_" + flightID + "><td>" + flightID + "</td><td>"
            + flightPlan.company_name + "</td><td>" +
            flightPlan.passengers + "</td><td>" + "longitude: " + flightPlan.initial_location.longitude + " &emsp;"
            + "latitude: " + flightPlan.initial_location.latitude + "</td></tr>");
    })
        .fail(function (jqXHR) { toastr.error(jqXHR.statusText + ' : ' + jqXHR.responseText); })
}

function paintFlightPath(flightPlan, flightID) {
    //remove previous paths from map:
    if (flightPath.polyLine !== null) {
        flightPath.polyLine.setMap(null);
    }

    lines = [{ lat: Number(flightPlan.initial_location.latitude), lng: Number(flightPlan.initial_location.longitude) }];
    for (let i = 0; i < flightPlan.segments.length; i++) {
        let line = { lat: Number(flightPlan.segments[i].latitude), lng: Number(flightPlan.segments[i].longitude) };
        lines.push(line);
    }
    flightPath.polyLine = new google.maps.Polyline({
        path: lines,
        geodesic: true,
        strokeColor: '#4cff00',
        strokeOpacity: 1.0,
        strokeWeight: 2
    });
    flightPath.polyLine.setMap(map);
    flightPath.flightId = flightID;
}

//var drop = $('#dropZone');
const dropArea = document.getElementById('dropZone');

const preventDefaults = e => {
    e.preventDefault();
    e.stopPropagation();
}

const highlight = e => {
    dropArea.classList.add('highlight');
    $('#internalFlights').addClass('table-dark');
}

const unhighlight = e => {
    dropArea.classList.remove('highlight');
    $('#internalFlights').removeClass('table-dark');
}

const handleDrop = e => {
    const dt = e.dataTransfer;
    const file = dt.files;

    handleFiles(file);
}

const handleFiles = file => {
    if (file.length === 1 && file[0].type.includes('/json')) {
        var reader = new FileReader();

        reader.onload = function (e2) {
            // finished reading file data.
            let flightJson = atob(e2.target.result.replace('data:application/json;base64,', ''));

            // /api/FlightPlan
            $.ajax({
                url: '/api/FlightPlan',
                contentType: "application/json",
                type: "POST",
                data: flightJson
            })
                .done(function () {
                    location.reload();
                })
                .fail(function (res) {
                    toastr.error("Error" + res);
                });
        }

        reader.readAsDataURL(file[0]); // start reading the file data.
    } else {
        toastr.error("Accept Only 1 Json File");
    }
}

["dragenter", "dragover", "dragleave", "drop"].forEach(eventName => {
    dropArea.addEventListener(eventName, preventDefaults, false);
});

["dragenter", "dragover"].forEach(eventName => {
    dropArea.addEventListener(eventName, highlight, false);
});

["dragleave", "drop"].forEach(eventName => {
    dropArea.addEventListener(eventName, unhighlight, false)
});

dropArea.addEventListener("drop", handleDrop, false);

function deleteFlight(id) {
    $.ajax({
        url: '/api/Flights/' + id,
        contentType: "text/plain; charset=utf-8",
        type: "DELETE",
    }).done(function () {
        document.getElementById(id).remove();
        if (flightPath.flightId === id) {
            flightPath.polyLine.setMap(null);
            flightPath.flightId = null
        } else {
            //this case is relevant when the deleted flight is not the selected flight, so the selected flight need to remain selected.
            showFlight(flightPath.flightId);
        }
    }).fail(function (res) {
        toastr.error("Error" + res);
    });
}

// the smooth zoom function
function smoothZoom(map, max, cnt) {
    if (cnt >= max) {
        return;
    }
    else {
        z = google.maps.event.addListener(map, 'zoom_changed', function (event) {
            google.maps.event.removeListener(z);
            smoothZoom(map, max, cnt + 1);
        });
        setTimeout(function () { map.setZoom(cnt) }, 80); // 80ms is what I found to work well on my system -- it might not work well on all systems
    }
}



