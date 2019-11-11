using console.Models;

namespace console.Services
{
    public interface IMembershipService
    {
        public void ActivateMembership(Payment payment);
        public void UpgradeMembership(Payment payment);
    }
}