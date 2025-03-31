import { address } from "../../shared/constants.js";
import { request } from "../../shared/utils.js";

export function getDevicesOnUserId(id) {
    fetch(`${address}/get-on-user-id`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ id: id })
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

export function update(ids) {
    fetch(`${address}/get-on-user-id`, {
        method: "PATCH",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ ids: ids })
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

export function getLogsOnDeviceId(id) {
    return request("GET", `/device/logs/${id}`);
}
