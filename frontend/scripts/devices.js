import { add } from "./services/devices.service.js";
import { devices } from "../mockdata/devices.mockdata.js";
import { logout } from "../shared/utils.js";

// getDevices().then(res => {
//     buildTable(res)
// })

buildTable(devices);

let selectedReferenceId = null; // Store the selected referenceId

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
            <td>Temperature: ${device.latestLog.temperature}°C, Date: ${device.latestLog.date}</td>
            <td style="width: 90px;">
                <img class="editIconbtn tableIcons" src="/img/Edit.png" data-referenceid="${device.referenceId}">
                <img class="trashBtn tableIcons" src="/img/Trash.png" data-referenceid="${device.referenceId}">
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
            selectedReferenceId = this.getAttribute("data-referenceid"); // Store referenceId
            document.getElementById("deleteDeviceHeader").innerHTML = `Delete Device ${selectedReferenceId}`;
            document.getElementById("deleteModal").style.display = "block";
        };
    });

    // Attach click event to all trash buttons
    document.querySelectorAll(".editIconbtn").forEach((btn) => {
      btn.onclick = function () {
          selectedReferenceId = this.getAttribute("data-referenceid"); // Store referenceId
          document.getElementById("editDeviceHeader").innerHTML = `Edit Device ${selectedReferenceId}`;
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
    if (selectedReferenceId) {
        deleteDevice(selectedReferenceId); // Call delete function with referenceId
        document.getElementById("deleteModal").style.display = "none";
        window.location.reload();
    }
};

document.getElementById("addbtn").onclick = () => {
    const referenceId = document.getElementById("referenceId").value;
      add(referenceId); // Call delete function with referenceId
      document.getElementById("deleteModal").style.display = "none";
      window.location.reload();
};

document.getElementById("editbtn").onclick = () => {
  if (selectedReferenceId) {
    const name = document.getElementById("name").value;
    const tempHigh = document.getElementById("tempHigh").value;
    const tempLow = document.getElementById("tempLow").value;

    update(name, tempHigh, tempLow, selectedReferenceId); // Call delete function with referenceId
      document.getElementById("editModal").style.display = "none";
      window.location.reload();
  }
};

document.querySelector(".logout-container").addEventListener("click", logout);

