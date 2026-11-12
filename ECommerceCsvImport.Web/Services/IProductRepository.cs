using ECommerceCsvImport.Web.Models;

namespace ECommerceCsvImport.Web.Services
{
    public interface IProductRepository
    {
        void Add(Product product);
        IReadOnlyList<Product> GetAll();
        Product? GetBySku(string sku);
        int Count { get; }
        void Clear();
    }
}