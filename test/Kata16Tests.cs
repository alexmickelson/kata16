using System.Collections.Generic;
using console;
using console.Enums;
using console.Models;
using console.Services;
using Moq;
using NUnit.Framework;

namespace test
{
    public class Kata16Tests
    {
        private Mock<IPackingSlipBuilder> packingSlipBuilderMoq;
        private Mock<IProductCatalog> productCatalogMoq;
        private Mock<IMembershipService> membershipMoq;
        private Mock<ICommisionService> commissionMoq;
        private PaymentHandler paymentHandler;
        private PaymentAccount defaultPaymentAccout;

        [SetUp]
        public void Setup()
        {
            // mailMoq = new Mock<IMailService>();
            packingSlipBuilderMoq = new Mock<IPackingSlipBuilder>();
            productCatalogMoq = new Mock<IProductCatalog>();
            membershipMoq = new Mock<IMembershipService>();
            commissionMoq = new Mock<ICommisionService>();
            paymentHandler = new PaymentHandler(
                                () => { return packingSlipBuilderMoq.Object; },
                                productCatalogMoq.Object,
                                membershipMoq.Object,
                                commissionMoq.Object);
            defaultPaymentAccout = new PaymentAccount()
            {

            };
        }

        [Test]
        public void GeneratePackingSlipForPhysicalProducts()
        {
            var physicalProductId = 4;
            var payment = new Payment(){
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=physicalProductId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(physicalProductId))
                             .Returns(new List<PaymentTags>(){PaymentTags.PhysicalProduct});
            packingSlipBuilderMoq.Setup(m => m.SendPackingSlipTo(Departments.Warehouse)).Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(packingSlipBuilderMoq);
        }

        [Test]
        public void GeneratePackingSlipsForBook()
        {
            var bookProductId = 3;
            var payment = new Payment()
            {
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=bookProductId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(bookProductId))
                             .Returns(new List<PaymentTags>()
                             {
                                PaymentTags.PhysicalProduct,
                                PaymentTags.Book
                             });
            packingSlipBuilderMoq.Setup(m => m.SendPackingSlipTo(Departments.Warehouse)).Verifiable();
            packingSlipBuilderMoq.Setup(m => m.SendPackingSlipTo(Departments.Royalty)).Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(packingSlipBuilderMoq);
        }

        [Test]
        public void ActivateMemebershipWhenPurchased()
        {
            var membershipProductId = 3;
            var payment = new Payment()
            {
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=membershipProductId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(membershipProductId))
                             .Returns(new List<PaymentTags>()
                             {
                                PaymentTags.NewMembership
                             });
            membershipMoq.Setup(ms => ms.ActivateMembership(payment))
                                .Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void UpgradeMembershipWhenPurchased()
        {
            var membershipUpgradeProductId = 4;
            var payment = new Payment()
            {
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=membershipUpgradeProductId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(membershipUpgradeProductId))
                             .Returns(new List<PaymentTags>()
                             {
                                PaymentTags.MembershipUpgrade
                             });
            membershipMoq.Setup(ms => ms.UpgradeMembership(payment))
                                .Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void NotifyConsumerOnMembershipPurchase()
        {
            var MemberhipPurchaseId = 4;
            var payment = new Payment()
            {
                Id=1,
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=MemberhipPurchaseId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(MemberhipPurchaseId))
                             .Returns(new [] { PaymentTags.NewMembership });
            membershipMoq.Setup(m => m.NotifyUserOfMembershipModification(payment))
                         .Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void NotifyConsumerOnMembershipUpgrade()
        {
            var MemberhipUpgradeId = 4;
            var payment = new Payment()
            {
                Id=1,
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=MemberhipUpgradeId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(MemberhipUpgradeId))
                             .Returns(new [] { PaymentTags.MembershipUpgrade });
            membershipMoq.Setup(m => m.NotifyUserOfMembershipModification(payment))
                         .Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void AddFirstAidVideoWhenLearningToSki()
        {
            var learingToSkiVideo = 4;
            var firstAidVideoId = 4;
            var payment = new Payment()
            {
                Id=1,
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=learingToSkiVideo,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(learingToSkiVideo))
                             .Returns(new [] { PaymentTags.AddFirstAidVideo });
            productCatalogMoq.Setup(pc => pc.GetProductId(PaymentHandler.FirstAidVideo))
                             .Returns(firstAidVideoId);
            packingSlipBuilderMoq
                .Setup(m => m.AddItemToOrder(firstAidVideoId))
                .Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(packingSlipBuilderMoq);
        }

        [Test]
        public void GenerateCommisionForBook()
        {
            int bookProductId = 6;
            int agentId = 10;
            var payment = new Payment()
            {
                Id=1,
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=bookProductId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(bookProductId))
                             .Returns( new []{ 
                                 PaymentTags.Commission, PaymentTags.Book
                                 });
            productCatalogMoq.Setup(s => s.GetAgentId(bookProductId))
                             .Returns(agentId);
            commissionMoq.Setup(c => c.GenerateCommision(payment, agentId))
                         .Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(commissionMoq);
        }

        [Test]
        public void GenerateCommisionForPhysicalProduct()
        {
            int physicalProductId = 6;
            int agentId = 10;
            var payment = new Payment()
            {
                Id=1,
                PaymentAccount = defaultPaymentAccout,
                Amount=20.01M
            };
            var order = new Order(){
                OrderId = 100,
                ProductId=physicalProductId,
                Payment=payment
            };
            productCatalogMoq.Setup(pc => pc.GetTags(physicalProductId))
                             .Returns( new []{ 
                                 PaymentTags.Commission, PaymentTags.PhysicalProduct
                                 });
            productCatalogMoq.Setup(s => s.GetAgentId(physicalProductId))
                             .Returns(agentId);
            commissionMoq.Setup(c => c.GenerateCommision(payment, agentId))
                         .Verifiable();

            paymentHandler.Process(order);

            Mock.Verify(commissionMoq);
        }
    }
}