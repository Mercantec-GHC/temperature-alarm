import { add, getDevices, update, deleteDevice } from "./services/devices.service.js";
import { devices } from "../mockdata/devices.mockdata.js";
import { logout } from "../shared/utils.js";

getDevices().then(res => {
    if(!res.message){
        buildTable(res)
    }
})

let selectedId = null; // Store the selected referenceId
const nameInput = document.getElementById("name");
const tempHighInput = document.getElementById("tempHigh");
const tempLowInput = document.getElementById("tempLow");

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


    document.getElementById("addDevice").onclick = () => {
      document.getElementById("addModal").style.display = "block";
    }

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
        //   document.getElementById("editDeviceHeader").innerHTML = `Edit Device ${this.getAttribute("data-referenceId")}`;
          nameInput.value = this.getAttribute("data-name");
          tempHighInput.value = this.getAttribute("data-tempHigh");
          tempLowInput.value = this.getAttribute("data-tempLow");
          document.getElementById("editModal").style.display = "block";
      };
  });
}


document.querySelectorAll(".cancelbtn").forEach(button => {
  button.onclick = () => {
      document.getElementById("deleteModal").style.display = "none";
      document.getElementById("editModal").style.display = "none";
      document.getElementById("addModal").style.display = "none";

  };
});
// Delete button logic
document.getElementById("deletebtn").onclick = () => {
    if (selectedId) {
        deleteDevice(selectedId); // Call delete function with referenceId
        window.location.reload();
    }
};

document.getElementById("addbtn").onclick = () => {
    const referenceId = document.getElementById("referenceId").value;
      add(referenceId); // Call delete function with referenceId
     window.location.reload();
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
          location.href = "/devices";
        });
  }
};

document.querySelector(".logout-container").addEventListener("click", logout);

