using System.Globalization;
using ECommerceCsvImport.Web.Models;

namespace ECommerceCsvImport.Web.Services
{
    public class ProductImportService : IProductImportService
    {
        private readonly IProductRepository _repository;
        private const string RequiredHeader = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number";

        public ProductImportService(IProductRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public ImportResult ImportFromCsv(string filePath)
        {
            var result = new ImportResult();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.Errors.Add(new ImportError { RowNumber = 0, Message = "Файл не найден" });
                return result;
            }

            if (!File.Exists(filePath))
            {
                result.Errors.Add(new ImportError { RowNumber = 0, Message = "Файл не найден" });
                return result;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                return ImportFromLines(lines);
            }
            catch (Exception)
            {
                result.Errors.Add(new ImportError { RowNumber = 0, Message = "Ошибка при чтении файла" });
                return result;
            }
        }

        public ImportResult ImportFromLines(IEnumerable<string> lines)
        {
            var result = new ImportResult();
            var lineList = lines.ToList();

            if (lineList.Count == 0 || lineList.All(string.IsNullOrWhiteSpace))
            {
                result.Errors.Add(new ImportError { RowNumber = 0, Message = "Данные для импорта отсутствуют" });
                return result;
            }

            int headerIndex = lineList.FindIndex(l => !string.IsNullOrWhiteSpace(l));
            string headerLine = lineList[headerIndex];
            var headers = CsvLineParser.Parse(headerLine).Select(h => h.ToLowerInvariant()).ToList();
            var requiredColumns = RequiredHeader.Split(',');
            if (requiredColumns.Any(col => !headers.Contains(col)))
            {
                result.Errors.Add(new ImportError { RowNumber = headerIndex + 1, Message = "Неверные заголовки CSV" });
                return result;
            }
            var columnMap = new Dictionary<string, int>();
            for (int i = 0; i < headers.Count; i++)
            {
                if (!columnMap.ContainsKey(headers[i]))
                    columnMap[headers[i]] = i;
            }

            var importedSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int expectedColumnCount = headers.Count;

            for (int i = headerIndex + 1; i < lineList.Count; i++)
            {
                string line = lineList[i];
                if (string.IsNullOrWhiteSpace(line)) continue; 

                result.TotalRows++;
                int rowNumber = i + 1; 

                var values = CsvLineParser.Parse(line);

                if (values.Count != expectedColumnCount)
                {
                    result.Errors.Add(new ImportError { RowNumber = rowNumber, Message = "Неверное количество полей в строке" });
                    continue;
                }

                string GetValue(string colName) =>
                    columnMap.TryGetValue(colName, out int idx) && idx < values.Count ? values[idx] : string.Empty;

                var rowErrors = new List<ImportError>();

                string sku = GetValue("sku").Trim();
                string name = GetValue("name").Trim();
                string desc = GetValue("description").Trim();
                string priceStr = GetValue("price").Trim();
                string currency = GetValue("currency").Trim();
                string category = GetValue("category_name").Trim();
                string stockStr = GetValue("stock").Trim();
                string imageUrl = GetValue("image_url").Trim();
                string isActiveStr = GetValue("is_active").Trim();
                string brand = GetValue("brand").Trim();
                string warrantyStr = GetValue("warranty_months").Trim();
                string modelNumber = GetValue("model_number").Trim();


                if (string.IsNullOrEmpty(sku))
                    rowErrors.Add(new ImportError { RowNumber = rowNumber, Message = "Поле sku не может быть пустым" });

                if (string.IsNullOrEmpty(name))
                    rowErrors.Add(new ImportError { RowNumber = rowNumber, Message = "Поле name не может быть пустым" });

                if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price <= 0)
                    rowErrors.Add(new ImportError { RowNumber = rowNumber, Message = "Поле price должно быть числом больше 0" });

                if (!int.TryParse(stockStr, out int stock) || stock < 0)
                    rowErrors.Add(new ImportError { RowNumber = rowNumber, Message = "Остаток на складе должен быть целым числом не меньше 0" });

                if (!TryParseBool(isActiveStr, out bool isActive))
                    rowErrors.Add(new ImportError { RowNumber = rowNumber, Message = "Поле is_active должно иметь значение true, false, 1 или 0" });

                if (string.IsNullOrEmpty(brand))
                    rowErrors.Add(new ImportError { RowNumber = rowNumber, Message = "Поле brand обязательно для варианта «Электроника»" });

                if (!int.TryParse(warrantyStr, out int warranty) || warranty < 0 || warranty > 60)
                    rowErrors.Add(new ImportError { RowNumber = rowNumber, Message = "Гарантия должна быть целым числом от 0 до 60 месяцев" });

                if (rowErrors.Count > 0)
                {
                    result.Errors.AddRange(rowErrors);
                    continue;
                }

                if (_repository.GetBySku(sku) != null || importedSkus.Contains(sku))
                {
                    result.Errors.Add(new ImportError { RowNumber = rowNumber, Message = "Артикул товара уже существует" });
                    continue;
                }

                var product = new Product
                {
                    Sku = sku,
                    Name = name,
                    Description = desc,
                    Price = price,
                    Currency = string.IsNullOrEmpty(currency) ? "RUB" : currency.ToUpperInvariant(),
                    CategoryName = string.IsNullOrEmpty(category) ? "Без категории" : category,
                    StockQuantity = stock,
                    ImageUrl = imageUrl,
                    IsActive = isActive,
                    Brand = brand,
                    WarrantyMonths = warranty,
                    ModelNumber = modelNumber
                };

                _repository.Add(product);
                importedSkus.Add(sku);
                result.ImportedCount++;
            }

            return result;
        }
        private static bool TryParseBool(string value, out bool result)
        {
            value = value.Trim().ToLowerInvariant();
            if (value == "true" || value == "1") { result = true; return true; }
            if (value == "false" || value == "0") { result = false; return true; }
            result = false;
            return false;
        }
    }
}