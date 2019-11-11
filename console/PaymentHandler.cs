using System.Linq;
using console.Models;
using console.Services;

namespace console
{
    public class PaymentHandler
    {
        private readonly IMailService mailService;
        private readonly IProductCatalog productCatalog;
        private readonly IMembershipService membershipService;

        public PaymentHandler(IMailService mailService,
                              IProductCatalog productCatalog,
                              IMembershipService membershipService)
        {
            this.mailService = mailService;
            this.productCatalog = productCatalog;
            this.membershipService = membershipService;
        }
        public void Process(Payment payment)
        {
            var tags = productCatalog.GetTags(payment.ProductId);
            if(tags.Contains(PaymentTags.PhysicalProduct))
            {
                mailService.GeneratePackingSlip(payment, Departments.Shipping);
            }
            if(tags.Contains(PaymentTags.Book))
            {
                mailService.GeneratePackingSlip(payment, Departments.Royalty);
            }
            if(tags.Contains(PaymentTags.Membership))
            {
                membershipService.ActivateMembership(payment);
            }
            if(tags.Contains(PaymentTags.MembershipUpgrade))
            {
                membershipService.UpgradeMembership(payment);
            }
        }
    }
}