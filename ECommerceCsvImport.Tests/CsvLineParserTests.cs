using ECommerceCsvImport.Web.Services;
using Xunit;

namespace ECommerceCsvImport.Tests
{
    public class CsvLineParserTests
    {
        [Fact]
        public void Parse_SimpleString_ReturnsFields()
        {
            var line = "1001,Ноутбук,45990";
            var result = CsvLineParser.Parse(line);

            Assert.Equal(3, result.Count);
            Assert.Equal("1001", result[0]);
            Assert.Equal("Ноутбук", result[1]);
        }

        [Fact]
        public void Parse_WithCommaInQuotes_ReturnsOneField()
        {
            var line = "1012,\"Ноутбук, игровой\",45990";
            var result = CsvLineParser.Parse(line);

            Assert.Equal(3, result.Count);
            Assert.Equal("Ноутбук, игровой", result[1]);
        }
    }
}