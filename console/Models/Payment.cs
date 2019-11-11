using System;
using System.Collections.Generic;

namespace console.Models
{
    public class Payment
    {
        public string UserAccount { get; set; }
        public int ProductId { get; set; }
        public Decimal Amount { get; set; }
    }
}