using System.Collections.Generic;

namespace console.Services
{
    public interface IProductCatalog
    {
        public IEnumerable<string> GetTags(int productId);
    }
}