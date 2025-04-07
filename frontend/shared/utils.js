import { address } from "./constants.js";

export async function request(method, path, body = null) {
    const token = await checkTokens()
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
                // if (response.status === 401) {
                //     location.href = "/login";
                // }

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

export function checkTokens() {
    var token = document.cookie.match(/\bauth-token=([^;\s]+)/);
    if(token != null){
        return token[1]
    }
    const match = document.cookie.match(/\brefresh-token=([^;\s]+)/);
    token = match ? match[1] : null;
    console.log("refresh "+token); 
   if(token != null){
    return fetch(`${address}/user/refreshtoken/${token}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
    })
    .then(async response => {
        if (!response.ok) {
            window.location.href = "/login";
            return;
        }
    
        const json = await response.json()
    
        document.cookie = `auth-token=${json.token}; Path=/`;
        document.cookie = `refresh-token=${json.refreshToken}; Path=/`;
    
        return json.token;
    });
   }
   else{
    window.location.href = "/login";
   }
}

export function logout() {
    localStorage.removeItem("user");
    document.cookie = "auth-token=";
    document.cookie = "refresh-token=";
    window.location.href = "/";
}

export function getUser() {
    return JSON.parse(localStorage.getItem("user"));
}

export function isLoggedIn() {
    return (document.cookie.match(/\bauth-token=/) && localStorage.getItem("user"));  
}

