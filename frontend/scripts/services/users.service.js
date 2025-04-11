import { request } from "../../shared/utils.js";
import { address } from "../../shared/constants.js";


export function get() {
    return request("GET",`/user/get`)
}


export function login(emailOrUsrn, password){
    return request("POST", "/user/login", {
        emailOrUsrn,
        password,
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



