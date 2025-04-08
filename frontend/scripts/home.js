import { logout } from "../shared/utils.js";
import { getDevices, getLogsOnDeviceId } from "./services/devices.service.js";

let chart;

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

        const parsedDate = luxon.DateTime.fromISO(log.date, { zone: "UTC" }).setZone("Europe/Copenhagen").setLocale("gb");
        const date = parsedDate.toLocaleString(luxon.DateTime.DATE_SHORT);
        const time = parsedDate.toLocaleString(luxon.DateTime.TIME_WITH_SECONDS);

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

function addDeviceToDropdown(device) {
    const opt = document.createElement("option");
    opt.innerText = `${device.name} (${device.referenceId})`;
    opt.value = device.id;
    document.getElementById("device-selector").appendChild(opt);
}

function randomColorChannelValue() {
    return Math.floor(Math.random() * 256);
}

function isSameDay(a, b) {
    return a.getFullYear() === b.getFullYear() &&
        a.getMonth() === b.getMonth() &&
        a.getDate() === b.getDate();
}

function localToUTC(date) {
    if (!date) return null;

    return luxon.DateTime.fromISO(date, { zone: "Europe/Copenhagen" }).setZone("UTC");
}

async function fetchData() {
    document.body.classList.add("loading");

    const startDate = localToUTC(document.getElementById("period-start").value);
    const endDate = localToUTC(document.getElementById("period-end").value);

    const deviceData = [];

    for (const device of devices) {
        const data = await getLogsOnDeviceId(device.id, startDate, endDate)
            .catch(handleError);

        deviceData.push(data);
    }

    if (deviceData.length === 0) {
        return;
    }

    document.getElementById("table-body").innerHTML = "";
    buildTable(deviceData[0]);

    if (!chart) {
        chart = new Chart("chart", {
            type: "line",
            data: {
                datasets: [],
            },
            options: {
                responsive: false,
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
                        time: {
                            displayFormats: {
                                hour: "HH:mm",
                            },
                        },
                        adapters: {
                            date: {
                                zone: "system",
                            },
                        },
                    },
                },
            },
        });
    }

    chart.options.scales.x.time.unit = isSameDay(new Date(startDate), new Date(endDate))
        ? "hour"
        : "day";

    chart.data.datasets = deviceData.map((dataset, i) => {
        const color = new Array(3)
            .fill(null)
            .map(randomColorChannelValue)
            .join(",");

        return {
            label: devices[i].name,
            fill: false,
            lineTension: 0.4,
            backgroundColor: `rgba(${color}, 1.0)`,
            borderColor: `rgba(${color}, 0.1)`,
            data: dataset.map(log => ({
                x: luxon.DateTime.fromISO(log.date, { zone: "UTC" }).toMillis(),
                y: log.temperature,
            })),
        };
    });

    chart.update();

    document.body.classList.remove("loading");
}

function setPeriod(start, end) {
    const startDate = start && new Date(start);
    startDate?.setMinutes(startDate.getMinutes() - startDate.getTimezoneOffset());

    const endDate = start && new Date(end);
    endDate?.setMinutes(endDate.getMinutes() - endDate.getTimezoneOffset());

    document.getElementById("period-start").value = startDate?.toISOString().slice(0, 16);
    document.getElementById("period-end").value = endDate?.toISOString().slice(0, 16);

    fetchData();
}

function setPeriodLastDays(days) {
    const start = new Date()
    start.setDate(new Date().getDate() - days);
    start.setHours(0, 0, 0, 0);
    setPeriod(start, new Date().setHours(23, 59, 0, 0));
}

for (const elem of document.getElementsByClassName("last-x-days")) {
    elem.onclick = event => setPeriodLastDays(event.target.dataset.days);
}

for (const elem of document.querySelectorAll("#period-start, #period-end")) {
    elem.onchange = fetchData;
}

document.getElementById("all-time").onclick = () => setPeriod(null, null);

document.querySelector(".logout-container").addEventListener("click", logout);

const devices = await getDevices().catch(handleError);
for (const device of devices) {
    addDeviceToDropdown(device);
}

setPeriodLastDays(3);
fetchData();

