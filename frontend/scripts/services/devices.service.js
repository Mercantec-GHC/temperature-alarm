import { request } from "../../shared/utils.js";

export function getDevices() {
    return request("GET", "/device");
}

export function add(referenceId) {
    return request("POST", `/device/add/${referenceId}`);
}

export function deleteDevice(deviceId) {
    return request("DELETE", `/device/delete/${deviceId}`);
}

export function update(deviceId, name, temphigh, tempLow) {
    return request("PUT", `/device/update/${deviceId}`,{
        name,
        temphigh,
        tempLow
    });
}

export function getLogsOnDeviceId(id, startDate = null, endDate = null) {
    const query = startDate && endDate ? `?dateTimeStart=${startDate}&dateTimeEnd=${endDate}` : "";
    return request("GET", `/device/logs/${id}${query}`);
}
