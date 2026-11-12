using ECommerceCsvImport.Web.Models;

namespace ECommerceCsvImport.Web.Services
{
    public interface IProductImportService
    {
        ImportResult ImportFromCsv(string filePath);
        ImportResult ImportFromLines(IEnumerable<string> lines);
    }
}