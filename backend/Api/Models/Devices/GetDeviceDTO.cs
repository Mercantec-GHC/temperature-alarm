namespace Api.Models.Devices
{
    public class GetDeviceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TempHigh { get; set; }
        public double TempLow { get; set; }
        public string? ReferenceId { get; set; }
        public TemperatureLogs LatestLog { get; set; }
    }
}
