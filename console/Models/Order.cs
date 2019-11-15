using console.Enums;

namespace console.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public Payment Payment { get; set; }

        public int ProductId { get; set; }
    }
}