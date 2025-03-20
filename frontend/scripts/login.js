import { login } from "./services/users.service.js";

document.getElementById("loginForm").addEventListener("submit", function(event) {
    event.preventDefault();

    const emailOrUsername = document.getElementById("emailorusn").value;
    const password = document.getElementById("psw").value;
    
    login(emailOrUsername, password);
});