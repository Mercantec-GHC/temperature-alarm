import { address } from "../../shared/constants.js";

export function login(usernameOrEmail, password) {
    console.log(usernameOrEmail);
    console.log(password);

    fetch(`${address}/user/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({usernameOrEmail: usernameOrEmail, password: password})
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

export function create(email, username, password, repeatPassword){
    fetch(`${address}/user/create`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({email: email, username: username, password: password, repeatPassword: repeatPassword})
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}

 function update(email, username, password){
    fetch(`${address}/user/update`, {
        method: "PATCH",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({email: email, username: username, password: password})
    })
    .then(response => response.json())
    .then(data => console.log("Success:", data))
    .catch(error => console.error("Error:", error));
}
