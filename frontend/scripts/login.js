import { login } from "./services/users.service.js";

document.getElementById("loginForm").addEventListener("submit", function(event) {
    event.preventDefault();

    document.getElementById("form-error").style.display = "none";

    const emailOrUsername = document.getElementById("emailorusn").value;
    const password = document.getElementById("psw").value;
    
    login(emailOrUsername, password)
        .then(response => {
            if (response.error) {
                document.getElementById("form-error").innerText = response.error;
                document.getElementById("form-error").style.display = "block";

                return;
            }

            location.href = "/home";
        });
});
