using System.IO;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;

namespace XlsxCsvConversions.Converters
{
    public class CsvToXlsxConverter
    {
        public async Task Convert(string csvName, string xlsxFile, string sheetName)
        {
            using var reader = new StreamReader(csvName);
            var doc = await ReadXlsxFile(xlsxFile);
            var sheet = doc.CreateSheet(sheetName);
            var rowIndex = 0;
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                var data = line.Split(',');
                var row = sheet.CreateRow(rowIndex);
                var cellIndex = 0;
                foreach (var value in data)
                {
                    row.CreateCell(cellIndex).SetCellValue(value);
                    cellIndex++;
                }

                rowIndex++;
            }

            var outputFile = new FileStream(xlsxFile, FileMode.OpenOrCreate, FileAccess.Write);
            doc.Write(outputFile, false);
        }

        private static async Task<XSSFWorkbook> ReadXlsxFile(string xlsxFile)
        {
            if (File.Exists(xlsxFile))
            {
                await using var outputFile = new FileStream(xlsxFile, FileMode.Open, FileAccess.Read);
                return new XSSFWorkbook(outputFile);
            }

            return new XSSFWorkbook();
        }
    }
}
