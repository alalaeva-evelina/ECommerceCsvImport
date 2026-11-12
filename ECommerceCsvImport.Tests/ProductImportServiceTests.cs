using ECommerceCsvImport.Web.Models;
using ECommerceCsvImport.Web.Services;
using Xunit;

namespace ECommerceCsvImport.Tests
{
    public class ProductImportServiceTests
    {
        private const string Header = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number";

        [Fact]
        public void ImportFromLines_ValidLine_ImportsProduct()
        {
            var repo = new InMemoryProductRepository();
            var service = new ProductImportService(repo);

            var lines = new[] {
                Header,
                "1001,Ноутбук Lenovo,Бюджетный,45990,RUB,Компьютеры,5,/img.jpg,true,Lenovo,24,IdeaPad"
            };

            var result = service.ImportFromLines(lines);

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(1, result.ImportedCount);
            Assert.Empty(result.Errors);
            Assert.Equal(1, repo.Count);
        }

        [Fact]
        public void ImportFromLines_InvalidPrice_AddsError()
        {
            var repo = new InMemoryProductRepository();
            var service = new ProductImportService(repo);

            var lines = new[] {
                Header,
                "1001,Ноутбук,Бюджетный,-500,RUB,Компьютеры,5,/img.jpg,true,Lenovo,24,IdeaPad"
            };

            var result = service.ImportFromLines(lines);

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(0, result.ImportedCount);
            Assert.Single(result.Errors);
            Assert.Contains("price", result.Errors[0].Message);
            Assert.Equal(0, repo.Count);
        }
    }
}