

using console.Models;

namespace console.Services
{
    public interface IMailService
    {
        public void GeneratePackingSlip(Payment payment, string department);
    }
}