import { create } from "./services/users.service.js";

document.getElementById("registerForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevents default form submission

    document.getElementById("form-error").style.display = "none";

    // Get form values
    const email = document.getElementById("email").value;
    const username = document.getElementById("uname").value;
    const password = document.getElementById("psw").value;
    const repeatPassword = document.getElementById("rpsw").value;

    // Call function with form values
    create(email, username, password, repeatPassword)
        .then(response => {
            if (response?.error) {
                document.getElementById("form-error").innerText = response.error;
                document.getElementById("form-error").style.display = "block";

                return;
            }

            location.href = "/login";
        });
});
