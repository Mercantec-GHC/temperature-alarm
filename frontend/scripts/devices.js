import { getDevicesOnUserId } from "./services/devices.service.js";

let idlocation = localStorage.getItem("rememberLogin")
let id;
if(idlocation){
    id = localStorage.getItem("id");
}   
else{
    id = localStorage.getItem("id");
}
getDevicesOnUserId(id).then(res => {
    buildTable(res)
})


function buildTable(data) {
    var table = document.getElementById(`deviceTable`);
    data.forEach((device) => {
      var row = `  <tr>
                          <td>Name</td>
                          <td class="${color}">${device.id}</td>
                          <td>${device.name}</td>
                      </tr>`;
      table.innerHTML += row;
    });
  }