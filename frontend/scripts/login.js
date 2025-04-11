import { login } from "./services/users.service.js";

document.getElementById("loginForm").addEventListener("submit", function(event) {
    event.preventDefault();

    const emailOrUsername = document.getElementById("emailorusn").value;
    const password = document.getElementById("psw").value;
    
    login(emailOrUsername, password).then((response) => {
        if(response.token && response.refreshToken){
            document.cookie = `auth-token=${response.token}; Path=/`;
            document.cookie = `refresh-token=${response.refreshToken}; Path=/`;
            localStorage.setItem("user", JSON.stringify({
                id: response.id,
                username: response.userName,
            }));
            location.href = "/home";
        }
    })
    .catch(error => {
        document.getElementById("login-error").innerText = error;
        document.getElementById("login-error").style.display = "block";
    });
});

