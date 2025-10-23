using OfficeOpenXml;
using System.Collections.Concurrent;

namespace Bulk_Export_POC.Utilities
{
    public class FileProcessorEPPlus
    {

        private static readonly ConcurrentDictionary<string, object> _fileLocks =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public FileProcessorEPPlus()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public List<Dictionary<string, string>> ReadExcel(string filePath, string sheetName = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found.", filePath);

            var data = new List<Dictionary<string, string>>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = string.IsNullOrEmpty(sheetName)
                    ? package.Workbook.Worksheets[0]
                    : package.Workbook.Worksheets[sheetName];

                if (worksheet == null)
                    throw new ArgumentException($"Sheet '{sheetName}' not found.");

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                var headers = new List<string>();
                for (int col = 1; col <= colCount; col++)
                    headers.Add(worksheet.Cells[1, col].Text);

                for (int row = 2; row <= rowCount; row++)
                {
                    var rowDict = new Dictionary<string, string>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        rowDict[headers[col - 1]] = worksheet.Cells[row, col].Text;
                    }
                    data.Add(rowDict);
                }
            }

            return data;
        }

        public void WriteExcel(string filePath, List<Dictionary<string, string>> data, string sheetName = "Sheet1")
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Data is empty.");

            var fullPath = Path.GetFullPath(filePath);
            var fileLock = _fileLocks.GetOrAdd(fullPath, _ => new object());

            lock (fileLock)
            {
                var file = new FileInfo(filePath);

                using (var package = file.Exists ? new ExcelPackage(file) : new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets[sheetName] ?? package.Workbook.Worksheets.Add(sheetName);

                    var headers = new List<string>(data[0].Keys);

                    int startRow = 1;

                    if (worksheet.Dimension == null)
                    {
                        for (int col = 0; col < headers.Count; col++)
                            worksheet.Cells[1, col + 1].Value = headers[col];

                        startRow = 2;
                    }
                    else
                    {
                        startRow = worksheet.Dimension.End.Row + 1;
                    }

                    for (int row = 0; row < data.Count; row++)
                    {
                        for (int col = 0; col < headers.Count; col++)
                        {
                            data[row].TryGetValue(headers[col], out string value);
                            worksheet.Cells[startRow + row, col + 1].Value = value ?? string.Empty;
                        }
                    }

                    if (worksheet.Dimension.Start.Row == 1 && worksheet.Dimension.End.Row == data.Count + 1)
                    {
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }

                    package.SaveAs(file);
                }
            }
        }
    }
}
