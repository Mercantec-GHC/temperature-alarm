import { request } from "../../shared/utils.js";

export function getDevices() {
    return request("GET", "/device");
}

export function add(referenceId) {
    return request("POST", "/device/adddevice", {referenceId: referenceId});
}

export function deleteDevice(referenceId) {
    return request("DELETE", "/device", {referenceId: referenceId});
}

export function update(name, temphigh, tempLow, referenceId) {
    return request("PUT", "/device/edit", {name: name, temphigh: temphigh, tempLow: tempLow, referenceId: referenceId});
}

export function getLogsOnDeviceId(id, startDate = null, endDate = null) {
    const query = startDate && endDate ? `?dateTimeStart=${startDate}&dateTimeEnd=${endDate}` : "";
    return request("GET", `/device/logs/${id}${query}`);
}
