using System.Collections.Generic;
using console;
using console.Models;
using console.Services;
using Moq;
using NUnit.Framework;

namespace test
{
    public class Tests
    {
        private Mock<IMailService> mailMoq;
        private Mock<IProductCatalog> productCatalogMoq;
        private Mock<IMembershipService> membershipMoq;
        private PaymentHandler paymentHandler;
        private string UserAccount;

        [SetUp]
        public void Setup()
        {
            mailMoq = new Mock<IMailService>();
            productCatalogMoq = new Mock<IProductCatalog>();
            membershipMoq = new Mock<IMembershipService>();
            paymentHandler = new PaymentHandler(mailMoq.Object,
                                                productCatalogMoq.Object,
                                                membershipMoq.Object);
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
                             .Returns(new List<string>(){PaymentTags.PhysicalProduct});
            mailMoq.Setup(m => m.GeneratePackingSlip(payment, Departments.Shipping)).Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(mailMoq);
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
                             .Returns(new List<string>()
                             {
                                PaymentTags.PhysicalProduct,
                                PaymentTags.Book
                             });
            mailMoq.Setup(m => m.GeneratePackingSlip(payment, Departments.Shipping)).Verifiable();
            mailMoq.Setup(m => m.GeneratePackingSlip(payment, Departments.Royalty)).Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(mailMoq);
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
                             .Returns(new List<string>()
                             {
                                PaymentTags.Membership
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
                             .Returns(new List<string>()
                             {
                                PaymentTags.MembershipUpgrade
                             });
            membershipMoq.Setup(ms => ms.UpgradeMembership(payment))
                                .Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(membershipMoq);
        }

        [Test]
        public void AddFirstAidVideoWhenLearningToSkii()
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
                             .Returns(new List<string>()
                             {
                                PaymentTags.AddFirstAidVideo
                             });
            mailMoq.Setup(ms => ms.AddItemToOrder(payment, firstAidVideoId))
                                .Verifiable();

            paymentHandler.Process(payment);

            Mock.Verify(mailMoq);
        }
    }
}