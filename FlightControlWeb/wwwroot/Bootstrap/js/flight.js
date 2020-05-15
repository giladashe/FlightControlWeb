function getFlights() {
    fetch("api/Flights?relative_to=<DATE_TIME>&sync_all")
        .then(response => response.json())
        .then(appendItem)
        .catch(error => console.log(error))
}

function appendItem() {
    
}