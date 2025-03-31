import { address } from "./constants.js";

export async function request(method, path, body = null) {
    const token = document.cookie.match(/\bauth-token=([^;\s]+)/);

    return new Promise((resolve, reject) => {
        fetch(address + path, {
            method,
            headers: {
                "Content-Type": body ? "application/json" : undefined,
                "Authorization": token?.length > 1 ? `Bearer ${token[1]}` : undefined,
            },
            body: body ? JSON.stringify(body) : undefined,
        })
        .then(async response => {
            try {
                const json = await response.json();

                if (response.ok) return resolve(json);

                if (json.error) return reject(json.error);

                if (json.message) return reject(json.message);

                if (json.errors) return reject(Object.values(json.errors)[0][0]);
            } finally {
                reject("Request failed with HTTP code " + response.status);
            }
        })
        .catch(err => reject(err.message));
    });
}

