
using System;
using System.Collections.Generic;
using System.Linq;
using console.Enums;
using console.Models;
using console.Services;

namespace console
{
    public class OrderRepository
    {
        private IEnumerable<Order> orders;
        private readonly PaymentHandler paymentHandler;
        private readonly IProductCatalog productCatalog;
        private readonly IPaymentService paymentService;

        public OrderRepository(PaymentHandler paymentHandler,
                               IProductCatalog productCatalog,
                               IPaymentService paymentService)
        {
            orders = new Order[]{};
            this.paymentHandler = paymentHandler;
            this.productCatalog = productCatalog;
            this.paymentService = paymentService;
        }
        public void ProcessAllOrders()
        {
            foreach (var order in activeOrders())
            {
                StartProcessingOrder(order);
                PaymentIsReady(order);
                if (ProductInStock(order))
                {
                    paymentHandler.Process(order);
                    CompleteIfNotPurchaseOrder(order);
                }
                CompletePendingPurchaseOrders(order);
            }
        }

        private IEnumerable<Order> activeOrders()
        {
            return orders.Where(o => o.Status != OrderStatus.Canceled);
        }

        private void CompletePendingPurchaseOrders(Order order)
        {
            if (order.Status == OrderStatus.WaitingForInvoicePayment)
            {
                order.Status = paymentService.RecievedPayForPurchaseOrder(order.Payment)
                                ? OrderStatus.Completed
                                : OrderStatus.WaitingForInvoicePayment;
            }
        }

        private bool ProductInStock(Order order)
        {
            return productCatalog.IsInStock(order.ProductId)
                    && order.Status == OrderStatus.WaitingToBeInStock;
        }

        private void CompleteIfNotPurchaseOrder(Order order)
        {
            if (order.Payment.PaymentAccount.Type == PaymentAccountType.PurchaseOrder
                && !paymentService.RecievedPayForPurchaseOrder(order.Payment))
            {
                order.Status = OrderStatus.WaitingForInvoicePayment;
            }
            else
            {
                order.Status = OrderStatus.Completed;
            }
        }

        private void PaymentIsReady(Order order)
        {
            if (!paymentHandler.NeedToDelayOrderForPayment(order.Payment.PaymentAccount)
                                    && order.Status == OrderStatus.WaitingForPayment)
            {
                order.Status = OrderStatus.WaitingToBeInStock;
            }
        }

        private static void StartProcessingOrder(Order order)
        {
            if (order.Status == OrderStatus.Placed)
            {
                order.Status = OrderStatus.WaitingForPayment;
            }
        }

        public void Add(Order order)
        {
            orders = orders.Append(order);
        }

        public Order Get(int orderId)
        {
            return orders.FirstOrDefault(o => o.OrderId == orderId);
        }
    }
}