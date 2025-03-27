import { request } from "../../shared/utils.js";

export function login(usernameOrEmail, password) {
    return request("POST", "/user/login", {
        EmailOrUsrn: usernameOrEmail,
        Password: password,
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
    return request("PATCH", "/user/update", {
        email,
        username,
    });
}

export function updatePassword(oldPassword, newPassword){
    return request("PATCH", "/user/update-password", {
        oldPassword,
        newPassword,
    });
}

