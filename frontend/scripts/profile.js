import { profileData } from "../mockdata/profile.mockdata.js";
import { logout } from "../shared/utils.js";
import { get } from "./services/users.service.js";
import { update } from "./services/users.service.js";
import { updatePassword } from "./services/users.service.js";

let id = localStorage.getItem("id");

get(id).then(res => {
    var table = document.getElementById(`profileCard`);
table.innerHTML += `
    <div class="pfp">
        <img style="width:200px; height:200px" src="${profileData.pfp}">
    </div>
    <div class="userData">
        <h2>${res.userName}</h2>
        <h2>${res.email}</h2>
    </div>
</div>`;
})



var pswModal = document.getElementById("PasswordModal");
var editModal = document.getElementById("editModal");
var editIconbtn = document.getElementById("openEditModal");
var passwordBtn = document.getElementById("openPasswordModal");

// Open modals
editIconbtn.onclick = () => (editModal.style.display = "block");
passwordBtn.onclick = () => (pswModal.style.display = "block");

// Close modals when clicking on any close button
document.querySelectorAll(".close").forEach(closeBtn => {
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

document.getElementById("editForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevents default form submission

    document.getElementById("form-error").style.display = "none";

    // Get form values
    const email = document.getElementById("email").value;
    const username = document.getElementById("uname").value;

    // Call function with form values
    update(email, username, id)
        .then(response => {
            if (response?.error) {
                document.getElementById("form-error").innerText = response.error;
                document.getElementById("form-error").style.display = "block";

                return;
            }

            location.href = "/profile";
        });
});

document.getElementById("PasswordForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevents default form submission

    document.getElementById("form-error").style.display = "none";

    const oldPassword = document.getElementById("oldpsw").value;
    const newPassword = document.getElementById("psw").value;
    const repeatPassword = document.getElementById("rpsw").value;

    if (newPassword !== repeatPassword) {
        let errorDiv = document.getElementById("form-error");
        errorDiv.style.display = "block"; 
        errorDiv.innerText = "Passwords do not match!";
        return;
    }

    updatePassword(oldPassword, newPassword, id)
        .then(response => {
            //error messages do not work
            if (response.error) {
                document.getElementById("form-error").innerText = response.message;
                document.getElementById("form-error").style.display = "block";
                return;
            }
        });
});

document.querySelector(".logout-container").addEventListener("click", logout);

