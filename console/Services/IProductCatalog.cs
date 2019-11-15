using System.Collections.Generic;
using console.Enums;

namespace console.Services
{
    public interface IProductCatalog
    {
        public IEnumerable<PaymentTags> GetTags(int productId);
        public int GetProductId(string productName);
        public int GetAgentId(int productId);
        public bool IsInStock(int productId);
    }
}