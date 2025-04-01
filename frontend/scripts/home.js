import { logout } from "../shared/utils.js";
import { getUser } from "../shared/utils.js";
import { getDevices, getLogsOnDeviceId } from "./services/devices.service.js";

async function buildChart(data) {
    const xValues = data.map((log) =>
        new Date(log.date).toLocaleString()
    ); // Full Date labels
    const yValues = data.map((log) => log.temperature); // Temperature values
    new Chart("myChart", {
        type: "line",
        data: {
            labels: xValues,
            datasets: [
                {
                    label: "Temperature",
                    fill: false,
                    lineTension: 0.4,
                    backgroundColor: "rgba(0,0,255,1.0)",
                    borderColor: "rgba(0,0,255,0.1)",
                    data: yValues,
                },
            ],
        },
        options: {
            tooltips: {
                callbacks: {
                    title: function (tooltipItem) {
                        return `Date: ${tooltipItem[0].label}`;
                    },
                    label: function (tooltipItem) {
                        return `Temperature: ${tooltipItem.value}°C`;
                    },
                },
            },
        },
    });
}

function buildTable(data) {
    var table = document.getElementById(`TemperatureTable`);

    data.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

    data.forEach((log) => {
        var averageTemp = (log.tempHigh + log.tempLow) / 2.0;
        var color;
        if (log.temperature >= log.tempHigh) {
            color = "tempHigh";
        } else if (
            log.temperature < log.tempHigh &&
            log.temperature > averageTemp
        ) {
            color = "tempMidHigh";
        } else if (log.temperature <= log.tempLow) {
            color = "tempLow";
        } else if (log.temperature > log.tempLow && log.temperature < averageTemp) {
            color = "tempMidLow";
        } else {
            color = "tempNormal";
        }

        const date = new Date(log.date).toLocaleDateString();
        const time = new Date(log.date).toLocaleTimeString();

        table.innerHTML += `
            <tr>
                <td class="temperature ${color}">${log.temperature}&deg;C</td>
                <td>${date}</td>
                <td width="50%">${time}</td>
                <td width="50%">Min: <b class="low-limit">${log.tempLow}&deg;C</b>, Max: <b class="high-limit">${log.tempHigh}&deg;C</b></td>
            </tr>
        `;
    });
}

function handleError(err) {
    document.getElementById("error").innerText = err;
    document.getElementById("error").style.display = "block";
    document.getElementById("container").style.display = "none";
}

async function init() {
    const devices = await getDevices()
        .catch(handleError);

    const deviceData = [];

    for (const device of devices) {
        const data = await getLogsOnDeviceId(device.id)
            .catch(handleError);

        deviceData.push(data);
    }

    if (deviceData.length === 0) {
        return;
    }

    buildTable(deviceData[0]);

    new Chart("myChart", {
        type: "line",
        data: {
            datasets: deviceData.map(dataset => ({
                label: "Temperature",
                fill: false,
                lineTension: 0.4,
                backgroundColor: "rgba(0,0,255,1.0)",
                borderColor: "rgba(0,0,255,0.1)",
                data: dataset.map(log => ({
                    x: log.date,
                    y: log.temperature,
                })),
            })),
        },
        options: {
            plugins: {
                tooltip: {
                    callbacks: {
                        label: item => `Temperature: ${item.formattedValue}°C`,
                    },
                },
            },
            scales: {
                x: {
                    type: "time",
                },
            },
        },
    });
}

init();

document.querySelector(".logout-container").addEventListener("click", logout);

