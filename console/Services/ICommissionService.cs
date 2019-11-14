using console.Models;

namespace console.Services
{
    public interface ICommisionService
    {
        public void GenerateCommision(Payment payment, int agentId);
    }
}