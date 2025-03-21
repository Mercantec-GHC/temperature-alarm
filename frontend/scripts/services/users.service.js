import { address } from "../../shared/constants.js";
import { handleResponse } from "../../shared/utils.js";

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

function update(email, username, password){
    return fetch(`${address}/user/update`, {
        method: "PATCH",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({email: email, username: username, password: password})
    })
    .then(handleResponse)
    .catch(err => { error: err.message });
}

