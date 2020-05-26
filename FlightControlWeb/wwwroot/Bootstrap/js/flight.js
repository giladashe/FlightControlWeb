

//variables
var markers = new Array();
var flightPath = { flightId: null, polyLine: null }

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
    });
}

function getFlights() {

    let currentTime = new Date().toISOString().substr(0, 19);
    let timeFormat = currentTime + 'Z';
    let ask = "/api/Flights?relative_to=" + timeFormat + "&sync_all";
    $.getJSON(ask, function (data) {
        data.forEach(function (flight) {
            if (document.getElementById(flight.flight_id) === null) {
                if (!flight.is_external) {
                    var row = "<tr id=" + flight.flight_id + " onclick=showFlight('" + flight.flight_id + "') ><td>" + flight.flight_id
                        + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.date_time + "</td>"
                        + "<td><a href='#'><i class='fa fa-trash' onclick=deleteFlight(\"" + flight.flight_id + "\")></i ></a>" + "</td></tr>";
                    $("#internalFlightsBody").append(row);
                } else {
                    $("#externalFlightsBody").append("<tr onclick=showFlight('" + flight.flight_id + "')><td>" + flight.flight_id
                        + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.date_time + "</td></tr>");
                }
            }
        })
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



//<a href='#'><i class= 'fas fa-trash-alt delete_icon'></i ></a> 
function showFlight(flightID) {


    // remove green background of "table-success" from all internalFlights table and add it only to the selected row
    $("#internalFlights tr").removeClass('table-success');
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
        $("#flightDetailsBody").append("<tr><td>" + flightID + "</td><td>" + flightPlan.company_name + "</td><td>" +
            flightPlan.passengers + "</td><td>" + "longitude: " + flightPlan.initial_location.longitude + " &emsp;"
            + "latitude: " + flightPlan.initial_location.latitude + "</td></tr>");
    });
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

var drop = $('#dropZone');
drop.on('dragover', function (e) {
    e.preventDefault();
    e.stopPropagation();
});

drop.on('dragenter', function (e) {
    e.preventDefault();
    e.stopPropagation();
    //$('#dropZone *').css("pointer-events", "none");
    //$('#dropZone').css({
    //    "background": "rgba(0,153,255,1)",
    //});
    //$('#internalFlights').hide();
    //$('.drag-text').removeClass('d-none');    
}).on('dragleave dragend mouseout', function (e) {
    e.preventDefault();
    e.stopPropagation();
    //$('#dropZone').css({
    //    "background": "transparent"
    //});
    //$('#internalFlights').show();
    //$('.drag-text').addClass('d-none');
});

// Get file data on drop
drop.on('drop', function (e) {

    //dropZone.classList.remove("dragover");
    e.stopPropagation();
    e.preventDefault();
    var files = e.originalEvent.dataTransfer.files; // Array of all files
    if (files.length === 1 && files[0].type.includes('/json')) {
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
                    alert("Error" + res);
                });
        }

        reader.readAsDataURL(files[0]); // start reading the file data.
    } else {
        alert("Accept Only 1 Json File");
    }
});

function deleteFlight(id) {
    // /api/FlightPlan
    if (flightPath.flightId === id) {
        flightPath.polyLine.setMap(null);
        flightPath.flightId = null
    }

    $.ajax({
        url: '/api/Flights/' + id,
        contentType: "text/plain; charset=utf-8",
        type: "DELETE",
    }).done(function () {
        getFlights();
    }).fail(function (res) {
        alert("Error" + res);
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
