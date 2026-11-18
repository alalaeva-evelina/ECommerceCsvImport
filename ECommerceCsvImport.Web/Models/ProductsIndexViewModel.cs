namespace ECommerceCsvImport.Web.Models
{
    public class ProductsIndexViewModel
    {
        public IReadOnlyList<Product> Products { get; set; } = new List<Product>();
        public int TotalCount => Products.Count;
        public decimal TotalValue => Products.Sum(p => p.Price);

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedBrand { get; set; }

        public List<string> Categories { get; set; } = new();
        public List<string> Brands { get; set; } = new();
    }
}