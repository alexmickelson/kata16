using System.Linq;
using console.Models;
using console.Services;

namespace console
{
    public class PaymentHandler
    {
        private readonly IMailService mailService;
        private readonly IProductCatalog productCatalog;

        public PaymentHandler(IMailService mailService,
                              IProductCatalog productCatalog)
        {
            this.mailService = mailService;
            this.productCatalog = productCatalog;
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
            
        }
    }
}