using console.Models;

namespace console.Services
{
    public interface IPaymentService
    {
        public bool PaymentCharged(Payment payment);
        bool RecievedPayForPurchaseOrder(Payment payment);
    }
}