import { profileData } from "../mockdata/profile.mockdata.js";
import { logout } from "../shared/utils.js";
import { get } from "./services/users.service.js";
import { update } from "./services/users.service.js";
import { updatePassword } from "./services/users.service.js";

var username;
var email;

get().then((res) => {
    username = res.userName;
    email = res.email;
    var table = document.getElementById(`profileCard`);
    table.innerHTML += `
    <div class="pfp">
        <img style="width:200px; height:200px" src="${profileData.pfp}">
    </div>
    <div class="userData">
        <h2>${username}</h2>
        <h2>${email}</h2>
    </div>
</div>`;
});

const checkForChanges = () => {
    if (emailInput.value !== email || usernameInput.value !== username) {
        submitBtn.disabled = false; // Enable button if changes were made
    } else {
        submitBtn.disabled = true; // Disable button if no changes
    }
};

const emailInput = document.getElementById("email");
const usernameInput = document.getElementById("uname");
const submitBtn = document.getElementById("submitEditBtn");
var pswModal = document.getElementById("PasswordModal");
var editModal = document.getElementById("editModal");
var editIconbtn = document.getElementById("openEditModal");
var passwordBtn = document.getElementById("openPasswordModal");

emailInput.addEventListener("input", checkForChanges);
usernameInput.addEventListener("input", checkForChanges);

// Open modals
editIconbtn.onclick = () => {
    document.getElementById("uname").value = username;
    document.getElementById("email").value = email;
    submitBtn.disabled = true;
    editModal.style.display = "block";
};
passwordBtn.onclick = () => (pswModal.style.display = "block");

// Close modals when clicking on any close button
document.querySelectorAll(".close").forEach((closeBtn) => {
    closeBtn.onclick = () => {
        pswModal.style.display = "none";
        editModal.style.display = "none";
        document.getElementById("form-error").innerText = "";
        document.getElementById("form-error").style.display = "none";
    };
});

// Close modals when clicking outside
window.onclick = (event) => {
    if (event.target == pswModal || event.target == editModal) {
        pswModal.style.display = "none";
        editModal.style.display = "none";
        document.getElementById("form-error").innerText = "";
        document.getElementById("form-error").style.display = "none";
    }
};

document
    .getElementById("editForm")
    .addEventListener("submit", function (event) {
        event.preventDefault(); // Prevents default form submission

        document.getElementById("form-error").style.display = "none";

        // Call function with form values
        update(emailInput.value, usernameInput.value).then((response) => {
            if (response?.error) {
                document.getElementById("form-error").innerText = response.error;
                document.getElementById("form-error").style.display = "block";

                return;
            }

            location.href = "/profile";
        });
    });

document
    .getElementById("PasswordForm")
    .addEventListener("submit", function (event) {
        event.preventDefault(); // Prevents default form submission

        document.getElementById("password-error").style.display = "none";

        const oldPassword = document.getElementById("oldpsw").value;
        const newPassword = document.getElementById("psw").value;
        const repeatPassword = document.getElementById("rpsw").value;

        if (newPassword !== repeatPassword) {
            document.getElementById("password-error").style.display = "block";
            document.getElementById("password-error").innerText = "Passwords do not match!";
            return;
        }

        updatePassword(oldPassword, newPassword)
            .then(() => location.reload())
            .catch(error => {
                document.getElementById("password-error").innerText = error;
                document.getElementById("password-error").style.display = "block";
            });
    });

document.querySelector(".logout-container").addEventListener("click", logout);
