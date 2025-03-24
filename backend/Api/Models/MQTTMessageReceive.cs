namespace Api.Models
{
    public class MQTTMessageReceive
    {
        public double temperature { get; set; }

        public string device_id { get; set; }

        public int timestamp { get; set; }
    }
}
