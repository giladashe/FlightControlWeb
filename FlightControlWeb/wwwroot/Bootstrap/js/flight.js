getFlights();



function getFlights() {
    let currentTime = new Date().toISOString().substr(0, 19);
    let timeFormat = currentTime + 'Z';
    //let ask = "/api/Flights?relative_to=" + timeFormat + "&sync_all";  
    let ask = "/api/Flights?relative_to=2020-11-27T01:56:21Z&sync_all"
    fetch(ask)
        .then(response => response.json())
        .then(data => appendItem(data))
        .catch(error => console.log(error));
        //.then(data => console.log(data)); 
}

function appendItem(data) {
    //for (let i = 0; i < data.length; i++) {
    /*let flight = response[i];
    for (let property in flight) {
        alert('flight ' + i + ': ' + property + '=' + flight[property]);
    }*/
    //}
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

}

