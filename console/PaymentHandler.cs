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

        public bool NeedToDelayOrderForPayment(PaymentAccount paymentAccount)
        {

            return paymentAccount.Type == PaymentAccountType.NotCreditCard;
        }

        public void Process(Order order)
        {
            var tags = productCatalog.GetTags(order.ProductId);
            var packingSlip = packingSlipBuilder();
            newMembership(order.Payment, tags);
            upgradeMembership(order.Payment, tags);
            sendMembershipChangeNotification(order.Payment, tags);
            addFirstAidVideo(tags, packingSlip);
            sendSlipToWarehouse(tags, packingSlip);
            sendSlipToRoyalty(tags, packingSlip);
            generateCommision(order, tags);
        }

        private void sendMembershipChangeNotification(Payment payment, IEnumerable<PaymentTags> tags)
        {
            if(tags.Contains(PaymentTags.MembershipUpgrade) || tags.Contains(PaymentTags.NewMembership))
            {
                membershipService.NotifyUserOfMembershipModification(payment);
            }
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

        private void generateCommision(Order order, IEnumerable<PaymentTags> tags)
        {
            if (tags.Any(t => t == PaymentTags.Book || t == PaymentTags.PhysicalProduct))
            {
                commisionService.GenerateCommision(
                    order.Payment,
                    productCatalog.GetAgentId(order.ProductId));
            }
        }
    }
}