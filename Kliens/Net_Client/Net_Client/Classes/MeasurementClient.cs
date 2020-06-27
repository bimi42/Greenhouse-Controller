using System;
using System.Collections.Generic;
using System.Text;

namespace Net_Client.Classes
{
    public class MeasurementClient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public double MeasuredValue { get; set; }
        //public string type { get; set; }
        public int GreenHouseId { get; set; }
        public int SensorId { get; set; }
        public Type Type { get; set; }
        public override string ToString()
        {
            return $"{Name}\t\t{DateTime}\t\t{MeasuredValue}\t{Type}";
        }
    }
}
