import { mockTemperatureLogs } from "../mockdata/temperature-logs.mockdata.js"; // Import data

const xValues = mockTemperatureLogs.map((log) =>
  new Date(log.date).toLocaleString()
); // Full Date labels
const yValues = mockTemperatureLogs.map((log) => log.temperature); // Temperature values
buildTable(mockTemperatureLogs);
new Chart("myChart", {
  type: "line",
  data: {
    labels: xValues,
    datasets: [
      {
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

function buildTable(data) {
  var table = document.getElementById(`TemperatureTable`);
  data.forEach((log) => {
    var averageTemp = (log.tempHigh + log.tempLow) / 2.0;
    var color;
    if (log.temperature > log.tempHigh) {
      color = "tempHigh";
    } else if (
      log.temperature < log.tempHigh &&
      log.temperature > averageTemp
    ) {
      color = "tempMidHigh";
    } else if (log.temperature < log.tempLow) {
      color = "tempLow";
    } else if (log.temperature > log.tempLow && log.temperature < averageTemp) {
      color = "tempMidLow";
    } else {
      color = "tempNormal";
    }
    var row = `  <tr>
                        <td>Name</td>
                        <td class="${color}">${log.temperature}</td>
                        <td>${log.date}</td>
                        <td class="tempHigh">${log.tempHigh}</td>
                        <td class="tempLow">${log.tempLow}</td>
                    </tr>`;
    table.innerHTML += row;
  });
}