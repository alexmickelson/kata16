using System.Collections.Generic;

namespace console.Models
{
    public class PackingSlip
    {
        public Payment Payment {get; set;}
        public IEnumerable<int> Products { get; set; }
        public string DestinationAddress { get; set; }
    }
}