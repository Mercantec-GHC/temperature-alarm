import { profileData } from "../mockdata/profile.mockdata.js";

var table = document.getElementById(`profileCard`);
table.innerHTML += `
    <div class="pfp">
        <img style="width:200px; height:200px" src="${profileData.pfp}">
    </div>
    <div class="userData">
        <h2>${profileData.username}</h2>
        <h2>${profileData.email}</h2>
    </div>
</div>`;

var pswModal = document.getElementById("PasswordModal");
var editModal = document.getElementById("editModal");
var editBtn = document.getElementById("openEditModal");
var passwordBtn = document.getElementById("openPasswordModal");

// Open modals
editBtn.onclick = () => (editModal.style.display = "block");
passwordBtn.onclick = () => (pswModal.style.display = "block");

// Close modals when clicking on any close button
document.querySelectorAll(".close").forEach(closeBtn => {
    closeBtn.onclick = () => {
        pswModal.style.display = "none";
        editModal.style.display = "none";
    };
});

// Close modals when clicking outside
window.onclick = (event) => {
    if (event.target == pswModal || event.target == editModal) {
        pswModal.style.display = "none";
        editModal.style.display = "none";
    }
};

document.getElementById("editForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevents default form submission

    document.getElementById("form-error-edit").style.display = "none";

    // Get form values
    const email = document.getElementById("email").value;
    const username = document.getElementById("uname").value;

    // Call function with form values
    update(email, username)
        .then(response => {
            if (response?.error) {
                document.getElementById("form-error").innerText = response.error;
                document.getElementById("form-error").style.display = "block";

                return;
            }

            location.href = "/login";
        });
});

document.getElementById("PasswordForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevents default form submission

    document.getElementById("form-error-password").style.display = "none";

    // Get form values
    const oldPassword = document.getElementById("oldpsw").value;
    const newPassword = document.getElementById("psw").value;
    const repeatPassword = document.getElementById("rpsw").value;

    if (newPassword !== repeatPassword) {
        let errorDiv = document.getElementById("form-error-password");
        errorDiv.style.display = "block"; 
        errorDiv.innerText = "Passwords do not match!";
        return;
    }

    // Call function with form values
    update(email, username)
        .then(response => {
            if (response?.error) {
                document.getElementById("form-error").innerText = response.error;
                document.getElementById("form-error").style.display = "block";

                return;
            }

            location.href = "/login";
        });
});
