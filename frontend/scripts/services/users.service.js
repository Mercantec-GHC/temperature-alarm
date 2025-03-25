import { address } from "../../shared/constants.js";
import { handleResponse } from "../../shared/utils.js";


export function get(userId) {
    return fetch(`${address}/user/${userId}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json"
        },
    })
    .then(handleResponse)
    .catch(err => { error: err.message });
}

export function login(usernameOrEmail, password) {
    return fetch(`${address}/user/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            EmailOrUsrn: usernameOrEmail,
            Password: password,
        }),
    })
    .then(handleResponse)
    .catch(err => { error: err.message });
}

export function create(email, username, password, repeatPassword){
    return fetch(`${address}/user/create`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({email: email, username: username, password: password, repeatPassword: repeatPassword})
    })
    .then(handleResponse)
    .catch(err => { error: err.message });
}

export function update(email, username, userId){
    return fetch(`${address}/user/edit/${userId}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({email: email, username: username})
    })
    .then(handleResponse)
    .catch(err => { error: err.message });
}

export function updatePassword(oldPassword, newPassword, userId){
    return fetch(`${address}/user/change-password/${userId}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({oldPassword: oldPassword, newPassword: newPassword})
    })
    .then(handleResponse)
    .catch(err => { error: err.message });
}

