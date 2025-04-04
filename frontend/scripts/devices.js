import { add, getDevices, update, deleteDevice } from "./services/devices.service.js";
import { devices } from "../mockdata/devices.mockdata.js";
import { logout } from "../shared/utils.js";

getDevices().then(res => {
  buildTable(res)
})


let selectedId = null; // Store the selected referenceId
const nameInput = document.getElementById("name");
const tempHighInput = document.getElementById("tempHigh");
const tempLowInput = document.getElementById("tempLow");

const editbtn = document.getElementById("editbtn");

var name;
var tempHigh;
var tempLow;

function buildTable(data) {
    var table = document.getElementById("deviceTable");
    table.innerHTML = ""; // Clear existing rows before adding new ones

    data.forEach((device) => {
        var row = document.createElement("tr");
        row.innerHTML = `
            <td>${device.referenceId}</td>
            <td>${device.name}</td>
            <td>${device.tempHigh}</td>
            <td>${device.tempLow}</td>
            <td>Temperature: ${device.latestLog}°C, Date: ${device.latestLog}</td>
            <td style="width: 90px;">
                <img class="editIconbtn tableIcons" src="/img/Edit.png" data-id="${device.id}" data-referenceId="${device.referenceId}" data-name="${device.name}" data-tempHigh="${device.tempHigh}" data-tempLow="${device.tempLow}">
                <img class="trashBtn tableIcons" src="/img/Trash.png" data-id="${device.id}" data-referenceId="${device.referenceId}">
            </td>
        `;
        table.appendChild(row);
    });

    // Attach click event to all trash buttons
    document.querySelectorAll(".trashBtn").forEach((btn) => {
        btn.onclick = function () {
            selectedId = this.getAttribute("data-id"); // Store referenceId
            // document.getElementById("deleteDeviceHeader").innerHTML = `Delete Device ${this.getAttribute("data-referenceId")}`;
            document.getElementById("deleteModal").style.display = "block";
        };
    });



    // Attach click event to all trash buttons
    document.querySelectorAll(".editIconbtn").forEach((btn) => {
      btn.onclick = function () {
          selectedId = this.getAttribute("data-id"); // Store referenceId
        document.getElementById("editDeviceHeader").innerHTML = `Edit Device ${this.getAttribute("data-referenceId")}`;
          nameInput.value = this.getAttribute("data-name");
          tempHighInput.value = this.getAttribute("data-tempHigh");
          tempLowInput.value = this.getAttribute("data-tempLow");
          name = nameInput.value;
          tempHigh = tempHighInput.value;
          tempLow = tempLowInput.value;
          editbtn.disabled = true;
          document.getElementById("editModal").style.display = "block";

      };
  });


}

document.getElementById("addDevice").onclick = () => {
  document.getElementById("referenceId").value = "";
  document.getElementById("addModal").style.display = "block";
}


document.querySelectorAll(".cancelbtn").forEach(button => {
  button.onclick = () => {
      document.getElementById("deleteModal").style.display = "none";
      document.getElementById("editModal").style.display = "none";
      document.getElementById("addModal").style.display = "none";

  };
});


const checkForChanges = () => {
  if (nameInput.value !== name || tempHighInput.value !== tempHigh || tempLowInput.value !== tempLow) {
    editbtn.disabled = false; // Enable button if changes were made
  } else {
    editbtn.disabled = true; // Disable button if no changes
  }
};
nameInput.addEventListener("input", checkForChanges);
tempHighInput.addEventListener("input", checkForChanges);
tempLowInput.addEventListener("input", checkForChanges);

// Delete button logic
document.getElementById("deletebtn").onclick = () => {
    if (selectedId) {
        deleteDevice(selectedId).then((response) => {
          if (response?.error) {
            document.getElementById("form-error").innerText = response.error;
            document.getElementById("form-error").style.display = "block";
            return;
          }
          window.location.reload();
        });
    }
};

document.getElementById("addbtn").onclick = () => {
    const referenceId = document.getElementById("referenceId").value;
      add(referenceId).then((response) => {
        if (response?.error) {
          document.getElementById("form-error").innerText = response.error;
          document.getElementById("form-error").style.display = "block";
          return;
        }
        window.location.reload();
      });
};

document.getElementById("editbtn").onclick = () => {
  if (selectedId) {
    const name = document.getElementById("name").value;
    const tempHigh = document.getElementById("tempHigh").value;
    const tempLow = document.getElementById("tempLow").value;


    update(selectedId, name, tempHigh, tempLow).then((response) => {
          if (response?.error) {
            document.getElementById("form-error").innerText = response.error;
            document.getElementById("form-error").style.display = "block";
            return;
          }
          window.location.reload();
        });
  }
};

document.querySelector(".logout-container").addEventListener("click", logout);

