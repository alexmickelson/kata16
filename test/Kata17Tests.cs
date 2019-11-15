using console;
using console.Enums;
using console.Models;
using console.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace test
{
    public class Kata17Tests
    {
        private Mock<IPackingSlipBuilder> packingSlipBuilderMoq;
        private Mock<IProductCatalog> productCatalogMoq;
        private Mock<IMembershipService> membershipMoq;
        private Mock<ICommisionService> commissionMoq;
        private Mock<IPaymentService> paymentServiceMoq;
        private PaymentHandler paymentHandler;
        private PaymentAccount paymentAccount;
        private Payment payment;
        private int productId;
        private int orderId;
        private Order order;
        private OrderRepository orderRepository;

        [SetUp]
        public void Setup()
        {
            // mailMoq = new Mock<IMailService>();
            packingSlipBuilderMoq = new Mock<IPackingSlipBuilder>();
            productCatalogMoq = new Mock<IProductCatalog>();
            membershipMoq = new Mock<IMembershipService>();
            commissionMoq = new Mock<ICommisionService>();
            paymentServiceMoq = new Mock<IPaymentService>();
            paymentHandler = new PaymentHandler(
                                () => { return packingSlipBuilderMoq.Object; },
                                productCatalogMoq.Object,
                                membershipMoq.Object,
                                commissionMoq.Object);
            paymentAccount = new PaymentAccount()
            {
                Id = 50
            };
            payment = new Payment()
            {
                Id = 40,
                PaymentAccount = paymentAccount,
                Amount = 20.01M
            };
            productId = 3;
            orderId = 100;
            order = new Order()
            {
                OrderId = orderId,
                ProductId = productId,
                Payment = payment
            };

            orderRepository = new OrderRepository(paymentHandler,
                                                  productCatalogMoq.Object,
                                                  paymentServiceMoq.Object);
        }
        
        [Test]
        public void WaitForPaymentOnWebOrders()
        {
            paymentAccount.Type = PaymentAccountType.NotCreditCard;
            order.Status = OrderStatus.Placed;
            orderRepository.Add(order);
            productCatalogMoq.Setup(pc => pc.IsInStock(order.ProductId)).Returns(true);

            orderRepository.ProcessAllOrders();

            order.Status.Should().Be(OrderStatus.WaitingForPayment);
            orderRepository.Get(orderId).Should().Be(order);
        }
        
        [Test]
        public void DontWaitForPaymentOnCreditCardWebOrders()
        {
            paymentAccount.Type = PaymentAccountType.CreditCard;
            order.Status = OrderStatus.Placed;
            productCatalogMoq.Setup(pc => pc.IsInStock(order.ProductId)).Returns(true);
            orderRepository.Add(order);

            orderRepository.ProcessAllOrders();

            orderRepository.Get(order.OrderId).Status.Should().Be(OrderStatus.Completed);
        }

        [Test]
        public void IfGoodsNotInStockDontProcessCreditCard()
        {
            paymentAccount.Type = PaymentAccountType.CreditCard;
            order.Status = OrderStatus.Placed;
            productCatalogMoq.Setup(pc => pc.IsInStock(order.ProductId)).Returns(false);
            orderRepository.Add(order);

            orderRepository.ProcessAllOrders();

            order.Status.Should().Be(OrderStatus.WaitingToBeInStock);
        }

        [Test]
        public void ShipGoodsIfPayingByCheckAndGoodsInStock()
        {
            paymentAccount.Type = PaymentAccountType.Check;
            order.Status = OrderStatus.Placed;
            productCatalogMoq.Setup(pc => pc.IsInStock(order.ProductId)).Returns(true);
            orderRepository.Add(order);

            orderRepository.ProcessAllOrders();

            order.Status.Should().Be(OrderStatus.Completed);
        }

        [Test]
        public void DontShipGoodsIfPayingByCheckAndGoodsNotInStock()
        {
            paymentAccount.Type = PaymentAccountType.Check;
            order.Status = OrderStatus.Placed;
            productCatalogMoq.Setup(pc => pc.IsInStock(order.ProductId)).Returns(false);
            orderRepository.Add(order);

            orderRepository.ProcessAllOrders();

            order.Status.Should().Be(OrderStatus.WaitingToBeInStock);
        }

        [Test]
        public void ShipGoodsIfPurchaseOrderAndPaymentNotRecieved()
        {
            paymentAccount.Type = PaymentAccountType.PurchaseOrder;
            order.Status = OrderStatus.Placed;
            productCatalogMoq.Setup(pc => pc.IsInStock(order.ProductId)).Returns(true);
            paymentServiceMoq.Setup(ps => ps.RecievedPayForPurchaseOrder(payment)).Returns(false);
            orderRepository.Add(order);

            orderRepository.ProcessAllOrders();

            order.Status.Should().Be(OrderStatus.WaitingForInvoicePayment);
        }

        [Test]
        public void IfPaymentArrivesForPurchaseOrderMakeItComplete()
        {
            paymentAccount.Type = PaymentAccountType.PurchaseOrder;
            order.Status = OrderStatus.WaitingForInvoicePayment;
            paymentServiceMoq.Setup(ps => ps.RecievedPayForPurchaseOrder(payment)).Returns(true);
            orderRepository.Add(order);

            orderRepository.ProcessAllOrders();

            order.Status.Should().Be(OrderStatus.Completed);
        }

        [Test]
        public void DoNotProcessCanceledOrders()
        {
            paymentAccount.Type = PaymentAccountType.NotCreditCard;
            order.Status = OrderStatus.Canceled ;
            orderRepository.Add(order);

            orderRepository.ProcessAllOrders();

            order.Status.Should().Be(OrderStatus.Canceled);

        }
    }
}