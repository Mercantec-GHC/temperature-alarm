import { mockTemperatureLogs } from "../../mockdata/temperature-logs.mockdata.js"; // Import data

const xValues = mockTemperatureLogs.map(log => new Date(log.Date).toLocaleString()); // Full Date labels
const yValues = mockTemperatureLogs.map(log => log.Temperature); // Temperature values

new Chart("myChart", {
    type: "line",
    data: {
        labels: xValues,
        datasets: [{
            fill: false,
            lineTension: 0.4,
            backgroundColor: "rgba(0,0,255,1.0)",
            borderColor: "rgba(0,0,255,0.1)",
            data: yValues
        }]
    },
    options: {
        tooltips: {
            callbacks: {
                title: function(tooltipItem) {
                    return `Date: ${tooltipItem[0].label}`;
                },
                label: function(tooltipItem) {
                    return `Temperature: ${tooltipItem.value}°C`;
                },
            }
        }
    }
});
