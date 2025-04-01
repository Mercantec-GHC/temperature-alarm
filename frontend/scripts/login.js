import { login } from "./services/users.service.js";

document.getElementById("loginForm").addEventListener("submit", function(event) {
    event.preventDefault();

    document.getElementById("form-error").style.display = "none";

    const emailOrUsername = document.getElementById("emailorusn").value;
    const password = document.getElementById("psw").value;
    
    login(emailOrUsername, password)
        .then(response => {
            document.cookie = `auth-token=${response.token}; Path=/`;

            localStorage.setItem("user", JSON.stringify({
                id: response.id,
                username: response.userName,
            }));

            location.href = "/home";
        })
        .catch(error => {
            document.getElementById("form-error").innerText = error;
            document.getElementById("form-error").style.display = "block";
        });
});

