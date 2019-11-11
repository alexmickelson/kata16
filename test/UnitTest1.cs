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
        private PaymentHandler paymentHandler;

        [SetUp]
        public void Setup()
        {
            mailMoq = new Mock<IMailService>();
            productCatalogMoq = new Mock<IProductCatalog>();
            paymentHandler = new PaymentHandler(mailMoq.Object,
                                                productCatalogMoq.Object);
        }

        [Test]
        public void GeneratePackingSlipForPhysicalProducts()
        {
            var physicalProductId = 4;
            var payment = new Payment(){
                UserAccount="Me",
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
            var tags = new List<string>()
            {
                PaymentTags.PhysicalProduct,
                PaymentTags.Book
            };
            var payment = new Payment()
            {
                UserAccount="Me",
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
    }
}