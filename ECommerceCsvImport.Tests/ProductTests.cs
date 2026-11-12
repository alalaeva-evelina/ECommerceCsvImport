using ECommerceCsvImport.Web.Models;
using ECommerceCsvImport.Web.Services;
using Xunit;

namespace ECommerceCsvImport.Tests
{
    public class ProductTests
    {
        [Fact]
        public void Add_AddsProduct_GetAllContainsIt()
        {
            var repo = new InMemoryProductRepository();
            var product = new Product { Sku = "1001", Name = "Ноутбук", Price = 45990m };

            repo.Add(product);
            var all = repo.GetAll();

            Assert.Single(all);
            Assert.Equal("1001", all[0].Sku);
        }

        [Fact]
        public void GetBySku_FindsProduct_IgnoringCaseAndSpaces()
        {
            var repo = new InMemoryProductRepository();
            repo.Add(new Product { Sku = "1001", Name = "Ноутбук" });

            var result = repo.GetBySku(" 1001 ");

            Assert.NotNull(result);
            Assert.Equal("Ноутбук", result.Name);
        }
    }
}