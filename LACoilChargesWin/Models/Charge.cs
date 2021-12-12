using System;

namespace LACoil.Models
{
    public class Charge
    {
        public Location Location { get; set; }
        public DateTime CreationDate { get; set; }
        public string ProductName { get; set; }
        public string DeviceNumber { get; set; }
        public string DeviceAddress { get; set; }

        public int CartId { get; set; }
        public long CartNumber { get; set; }

        public Currency Currency { get; set; }
        public double Qty { get; set; }

        public int CheckID { get; set; }

        public double Price { get; set; }

        public double? ReturnQty { get; set; }
        public int? ReturnCheckID { get; set; }
        public DateTime? ReturnDate { get; set; }
    }


    public enum Location
    {
        HH = 1,
        RD = 2,
    }
}