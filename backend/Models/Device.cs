using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Device
    {
        public int Id { get; set; }

        public double TempHigh { get; set; }

        public double TempLow { get; set; }

        public string? UnikId { get; set; }

        public List<TemperatureLogs>? Logs { get; set; }
    }
}
