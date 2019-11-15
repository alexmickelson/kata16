using System;
using System.Collections.Generic;

namespace console.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public PaymentAccount PaymentAccount { get; set; }
        public Decimal Amount { get; set; }
    }
}