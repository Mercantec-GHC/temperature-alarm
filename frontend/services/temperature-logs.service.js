const address = "http://10.135.51.116/temperature-alarm-webapi/temperature-logs"

function getOnDeviceIds(ids) {
    fetch(`${address}/get-on-device-ids`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ ids: ids })
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}