import { create } from "../../services/users.service.js";

document.getElementById("registerForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevents default form submission

    // Get form values
    const email = document.getElementById("email").value;
    const username = document.getElementById("uname").value;
    const password = document.getElementById("psw").value;
    const repeatPassword = document.getElementById("rpsw").value;

    // Call function with form values
    create(email, username, password, repeatPassword);
});