using Microsoft.AspNetCore.Mvc;
using ECommerceCsvImport.Web.Models;
using ECommerceCsvImport.Web.Services;

namespace ECommerceCsvImport.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IProductImportService _importService;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            IProductRepository repository,
            IProductImportService importService,
            IWebHostEnvironment environment)
        {
            _repository = repository;
            _importService = importService;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index(decimal? minPrice, decimal? maxPrice, string? category, string? brand)
        {
            var allProducts = _repository.GetAll();

            var viewModel = new ProductsIndexViewModel
            {
                Categories = allProducts.Select(p => p.CategoryName).Distinct().OrderBy(c => c).ToList(),
                Brands = allProducts.Select(p => p.Brand).Distinct().OrderBy(b => b).ToList(),
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedCategory = category,
                SelectedBrand = brand
            };

            var filtered = allProducts.AsQueryable();

            if (minPrice.HasValue) filtered = filtered.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) filtered = filtered.Where(p => p.Price <= maxPrice.Value);
            if (!string.IsNullOrWhiteSpace(category)) filtered = filtered.Where(p => p.CategoryName == category);
            if (!string.IsNullOrWhiteSpace(brand)) filtered = filtered.Where(p => p.Brand == brand);

            viewModel.Products = filtered.ToList();
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new ImportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import(string filePath)
        {
            var viewModel = new ImportViewModel { FilePath = filePath };

            if (string.IsNullOrWhiteSpace(filePath))
            {
                viewModel.Result = new ImportResult();
                viewModel.Result.Errors.Add(new ImportError { RowNumber = 0, Message = "Укажите путь к CSV-файлу" });
                return View(viewModel);
            }

            string physicalPath = filePath;
            if (filePath.StartsWith("/"))
            {
                physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            }

            viewModel.Result = _importService.ImportFromCsv(physicalPath);
            return View(viewModel);
        }
    }
}