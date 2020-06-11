using System.IO;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace XlsxCsvConversions.Converters
{
    public class XlsxToCsvConverter
    {
        public async Task Convert(string xlsxFile, string sheetName, string csvName)
        {
            var doc = await OpenXlsx(xlsxFile);
            var sheet = doc.GetSheet(sheetName);
            await using var writer = new StreamWriter(csvName);
            foreach (IRow row in sheet)
            {
                if (row != null)
                {
                    var line = row.Cells;
                    await writer.WriteLineAsync(string.Join(',', line));
                }
            }
        }

        private static async Task<XSSFWorkbook> OpenXlsx(string xlsxFile)
        {
            await using var file = new FileStream(xlsxFile, FileMode.Open, FileAccess.Read);
            return new XSSFWorkbook(file);
        }
    }
}
