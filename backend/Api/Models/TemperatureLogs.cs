namespace Api.Models
{
    public class TemperatureLogs
    {
        public int Id { get; set; }

        public double Temperature { get; set; }

        public DateTime Date { get; set; }

        public double TempHigh { get; set; }

        public double TempLow { get; set; }
    }
}
