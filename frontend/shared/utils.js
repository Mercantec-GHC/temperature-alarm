import { address } from "./constants.js";

export async function request(method, path, body = null) {
    var token = document.cookie.match(/\bauth-token=([^;\s]+)/);
    if (token != null) {
        token = token[1];
    }
    else{
        await getTokenByReferenceId(method, path, body);
    }
    const headers = {};
    headers["Authorization"] = `Bearer ${token}`;

    if (body)
        headers["Content-Type"] = "application/json";

    return new Promise((resolve, reject) => {
        fetch(address + path, {
            method,
            headers,
            body: body ? JSON.stringify(body) : undefined,
        })
            .then(async response => {
                if (response.status === 401) {
                   await getTokenByReferenceId(method, path, body);
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

export async function getTokenByReferenceId(method, path, body = null) {
    console.log("gtbri");
    var token = document.cookie.match(/\brefresh-token=([^;\s]+)/);
    if (token != null) {
        return await fetch(`${address}/user/refreshtoken/${token[1]}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
        })
        .then(async response => {
            const json = await response.json()

            document.cookie = `auth-token=${json.token}; Path=/`;
            document.cookie = `refresh-token=${json.refreshToken}; Path=/`;
            console.log(json.token);

            return request(method, path, body);
        })
        .catch(error => {
            location.href = "/login";
        });
    }

}

export function logout() {
    localStorage.removeItem("user");
    document.cookie = "auth-token=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT";
    document.cookie = "refresh-token=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT";
    window.location.href = "/";
}

export function getUser() {
    return JSON.parse(localStorage.getItem("user"));
}

export function isLoggedIn() {
    return (document.cookie.match(/\bauth-token=/) && localStorage.getItem("user"));  
}

