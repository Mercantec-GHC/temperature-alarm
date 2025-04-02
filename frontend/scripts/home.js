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

const TABLE_PAGINATION_SIZE = 30;

function buildTable(data, offset = 0) {
    data.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

    const page = data.slice(offset, offset + TABLE_PAGINATION_SIZE);

    page.forEach(log => {
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

        document.getElementById("table-body").innerHTML += `
            <tr>
                <td class="temperature ${color}">${log.temperature}&deg;C</td>
                <td>${date}</td>
                <td width="50%">${time}</td>
                <td width="50%">Min: <b class="low-limit">${log.tempLow}&deg;C</b>, Max: <b class="high-limit">${log.tempHigh}&deg;C</b></td>
            </tr>
        `;
    });

    document.getElementById("shown-log-amount").innerText = Math.min(data.length, offset + TABLE_PAGINATION_SIZE);
    document.getElementById("total-log-amount").innerText = data.length;

    if (offset + TABLE_PAGINATION_SIZE >= data.length) {
        document.getElementById("view-more").style.display = "none";

        return;
    }

    document.getElementById("view-more").style.display = "block";

    document.getElementById("view-more").onclick = () => {
        buildTable(data, offset + TABLE_PAGINATION_SIZE);
    }
}

function handleError(err) {
    document.getElementById("error").innerText = err;
    document.getElementById("error").style.display = "block";
    document.getElementById("container").style.display = "none";
}

async function fetchData(startDate = null, endDate = null) {
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
                    x: new Date(log.date).getTime(),
                    y: log.temperature,
                })),
            })),
        },
        options: {
            parsing: false,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: item => `Temperature: ${item.formattedValue}°C`,
                    },
                },
                decimation: {
                    enabled: true,
                    algorithm: "lttb",
                    samples: window.innerWidth / 2,
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

fetchData();

document.querySelector(".logout-container").addEventListener("click", logout);

