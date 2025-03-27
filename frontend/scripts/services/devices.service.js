import { address } from "../../shared/constants.js";

export function getDevicesOnUserId(userId) {
    fetch(`${address}/device/${userId}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json"
        },
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

export function add(id) {
    fetch(`${address}/device`, {
        method: "CREATE",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ referenceId: id})
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

export function update(id, name, tempHigh, tempLow) {
    fetch(`${address}/device/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ name: name, tempHigh: tempHigh, tempLow: tempLow })
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

export function deleteDevice(referenceId) {
    console.log(referenceId)
    fetch(`${address}/device/${referenceId}`, {
        method: "DELETE",
        headers: {
            "Content-Type": "application/json"
        },
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

export function getLogsOnDeviceIds(id) {
    fetch(`${address}/get-on-device-ids`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ ids: id })
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}