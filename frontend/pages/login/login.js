import { login } from "../../services/users.service.js";

document.getElementById("loginForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevents default form submission

    // Get form values
    const emailOrUsername = document.getElementById("emailorusn").value;
    const password = document.getElementById("psw").value;
    

    // Call function with form values
    login(emailOrUsername, password);
});