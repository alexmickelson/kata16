using System;
using System.Collections.Generic;
using System.Linq;
using console.Enums;
using console.Models;
using console.Services;

namespace console
{
    public class PaymentHandler
    {
        public static string FirstAidVideo = "included first aid video";
        private readonly Func<IPackingSlipBuilder> packingSlipBuilder;
        private readonly IProductCatalog productCatalog;
        private readonly IMembershipService membershipService;
        private readonly ICommisionService commisionService;

        public PaymentHandler(Func<IPackingSlipBuilder> _packingSlipBuilder,
                              IProductCatalog productCatalog,
                              IMembershipService membershipService,
                              ICommisionService commisionService)
        {
            packingSlipBuilder = _packingSlipBuilder;
            this.productCatalog = productCatalog;
            this.membershipService = membershipService;
            this.commisionService = commisionService;
        }

        public void Process(Payment payment)
        {
            var tags = productCatalog.GetTags(payment.ProductId);
            var packingSlip = packingSlipBuilder();
            newMembership(payment, tags);
            upgradeMembership(payment, tags);
            addFirstAidVideo(tags, packingSlip);
            sendSlipToWarehouse(tags, packingSlip);
            sendSlipToRoyalty(tags, packingSlip);
            generateCommision(payment, tags);
        }

        private void sendSlipToRoyalty(IEnumerable<PaymentTags> tags, IPackingSlipBuilder packingSlip)
        {
            if (tags.Contains(PaymentTags.Book))
            {
                packingSlip.SendPackingSlipTo(Departments.Royalty);
            }
        }

        private void sendSlipToWarehouse(IEnumerable<PaymentTags> tags, IPackingSlipBuilder packingSlip)
        {
            if (tags.Contains(PaymentTags.PhysicalProduct))
            {
                packingSlip.SendPackingSlipTo(Departments.Warehouse);
            }
        }

        private void addFirstAidVideo(IEnumerable<PaymentTags> tags, IPackingSlipBuilder packingSlip)
        {
            if (tags.Contains(PaymentTags.AddFirstAidVideo))
            {
                packingSlip.AddItemToOrder(productCatalog.GetProductId(FirstAidVideo));
            }
        }

        private void upgradeMembership(Payment payment, IEnumerable<PaymentTags> tags)
        {
            if (tags.Contains(PaymentTags.MembershipUpgrade))
            {
                membershipService.UpgradeMembership(payment);
            }
        }

        private void newMembership(Payment payment, IEnumerable<PaymentTags> tags)
        {
            if (tags.Contains(PaymentTags.NewMembership))
            {
                membershipService.ActivateMembership(payment);
            }
        }

        private void generateCommision(Payment payment, IEnumerable<PaymentTags> tags)
        {
            if (tags.Any(t => t == PaymentTags.Book || t == PaymentTags.PhysicalProduct))
            {
                commisionService.GenerateCommision(
                    payment,
                    productCatalog.GetAgentId(payment.ProductId));
            }
        }
    }
}