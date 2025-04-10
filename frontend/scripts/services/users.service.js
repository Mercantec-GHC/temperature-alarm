import { request } from "../../shared/utils.js";
import { address } from "../../shared/constants.js";


export function get() {
    return request("GET",`/user/get`)
}

export function login(usernameOrEmail, password) {
    return fetch(`${address}/user/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ password: password, EmailOrUsrn: usernameOrEmail })
    })
    .then(response => {
        if (!response.ok) {
            return("Request failed with HTTP code " + response.status);
        }
        return response.json();
    });
}

export function create(email, username, password, repeatPassword){
    return request("POST", "/user/create", {
        email,
        username,
        password,
        repeatPassword,
    });
}

export function update(email, username){
    return request("PUT", "/user/update", {
        email,
        username,
    });
}

export function updatePassword(oldPassword, newPassword){
    return request("PUT", "/user/change-password", {
        oldPassword,
        newPassword,
    });
}



