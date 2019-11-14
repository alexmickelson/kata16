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
        private string UserAccount;

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
            UserAccount = "me";
        }

        [Test]
        public void GeneratePackingSlipForPhysicalProducts()
        {
            var physicalProductId = 4;
            var payment = new Payment(){
                UserAccount=UserAccount,
                ProductId=physicalProductId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(physicalProductId))
                             .Returns(new List<PaymentTags>(){PaymentTags.PhysicalProduct});
            packingSlipBuilderMoq.Setup(m => m.SendPackingSlipTo(Departments.Warehouse)).Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(packingSlipBuilderMoq);
        }

        [Test]
        public void GeneratePackingSlipsForBook()
        {
            var bookProductId = 3;
            var payment = new Payment()
            {
                UserAccount=UserAccount,
                ProductId=bookProductId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(bookProductId))
                             .Returns(new List<PaymentTags>()
                             {
                                PaymentTags.PhysicalProduct,
                                PaymentTags.Book
                             });
            packingSlipBuilderMoq.Setup(m => m.SendPackingSlipTo(Departments.Warehouse)).Verifiable();
            packingSlipBuilderMoq.Setup(m => m.SendPackingSlipTo(Departments.Royalty)).Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(packingSlipBuilderMoq);
        }

        [Test]
        public void ActivateMemebershipWhenPurchased()
        {
            var membershipProductId = 3;
            var payment = new Payment()
            {
                UserAccount=UserAccount,
                ProductId=membershipProductId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(membershipProductId))
                             .Returns(new List<PaymentTags>()
                             {
                                PaymentTags.NewMembership
                             });
            membershipMoq.Setup(ms => ms.ActivateMembership(payment))
                                .Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void UpgradeMembershipWhenPurchased()
        {
            var membershipUpgradeProductId = 4;
            var payment = new Payment()
            {
                UserAccount=UserAccount,
                ProductId=membershipUpgradeProductId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(membershipUpgradeProductId))
                             .Returns(new List<PaymentTags>()
                             {
                                PaymentTags.MembershipUpgrade
                             });
            membershipMoq.Setup(ms => ms.UpgradeMembership(payment))
                                .Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void NotifyConsumerOnMembershipPurchase()
        {
            var MemberhipPurchaseId = 4;
            var payment = new Payment()
            {
                Id=1,
                UserAccount=UserAccount,
                ProductId=MemberhipPurchaseId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(MemberhipPurchaseId))
                             .Returns(new [] { PaymentTags.NewMembership });
            membershipMoq.Setup(m => m.NotifyUserOfMembershipModification(payment))
                         .Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void NotifyConsumerOnMembershipUpgrade()
        {
            var MemberhipUpgradeId = 4;
            var payment = new Payment()
            {
                Id=1,
                UserAccount=UserAccount,
                ProductId=MemberhipUpgradeId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(MemberhipUpgradeId))
                             .Returns(new [] { PaymentTags.MembershipUpgrade });
            membershipMoq.Setup(m => m.NotifyUserOfMembershipModification(payment))
                         .Verifiable();

            paymentHandler.Process(payment);

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
                UserAccount=UserAccount,
                ProductId=learingToSkiVideo,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(learingToSkiVideo))
                             .Returns(new [] { PaymentTags.AddFirstAidVideo });
            productCatalogMoq.Setup(pc => pc.GetProductId(PaymentHandler.FirstAidVideo))
                             .Returns(firstAidVideoId);
            packingSlipBuilderMoq
                .Setup(m => m.AddItemToOrder(firstAidVideoId))
                .Verifiable();

            paymentHandler.Process(payment);

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
                UserAccount=UserAccount,
                ProductId=bookProductId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(bookProductId))
                             .Returns( new []{ 
                                 PaymentTags.Commission, PaymentTags.Book
                                 });
            productCatalogMoq.Setup(s => s.GetAgentId(bookProductId))
                             .Returns(agentId);
            commissionMoq.Setup(c => c.GenerateCommision(payment, agentId))
                         .Verifiable();

            paymentHandler.Process(payment);

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
                UserAccount=UserAccount,
                ProductId=physicalProductId,
                Amount=20.01M
            };
            productCatalogMoq.Setup(pc => pc.GetTags(physicalProductId))
                             .Returns( new []{ 
                                 PaymentTags.Commission, PaymentTags.PhysicalProduct
                                 });
            productCatalogMoq.Setup(s => s.GetAgentId(physicalProductId))
                             .Returns(agentId);
            commissionMoq.Setup(c => c.GenerateCommision(payment, agentId))
                         .Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(commissionMoq);
        }
    }
}