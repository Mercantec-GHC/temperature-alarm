namespace Api.Models.Devices
{
    public class EditDeviceRequest
    {
        public string Name { get; set; }

        public double TempHigh { get; set; }

        public double TempLow { get; set; }
    }
}
