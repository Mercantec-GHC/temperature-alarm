import { address } from "./constants.js";

export async function request(method, path, body = null) {
    const token = document.cookie.match(/\bauth-token=([^;\s]+)/);

    const headers = {};
    if (body)
        headers["Content-Type"] = "application/json";
    if (token?.length > 1)
        headers["Authorization"] = `Bearer ${token[1]}`;

    return new Promise((resolve, reject) => {
        fetch(address + path, {
            method,
            headers,
            body: body ? JSON.stringify(body) : undefined,
        })
            .then(async response => {
                if (response.status === 401) {
                    location.href = "/login";
                }

                try {
                    const json = await response.json();

                    if (response.ok) return resolve(json);

                    if (json.error) return reject(json.error);

                    if (json.message) return reject(json.message);

                    if (json.title) return reject(json.title);

                    if (json.errors) return reject(Object.values(json.errors)[0][0]);
                } finally {
                    reject("Request failed with HTTP code " + response.status);
                }
            })
            .catch(err => reject(err.message));
    });
}

export function logout() {
    localStorage.removeItem("user");
    document.cookie = "auth-token=";
    window.location.href = "/";
}

export function getUser() {
    return JSON.parse(localStorage.getItem("user"));
}

