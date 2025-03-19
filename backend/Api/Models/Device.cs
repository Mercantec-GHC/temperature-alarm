namespace Api.Models
{
    public class Device
    {
        public int Id { get; set; }

        public double TempHigh { get; set; }

        public double TempLow { get; set; }

        public string? ReferenceId { get; set; }

        public List<TemperatureLogs>? Logs { get; set; }
    }
}
