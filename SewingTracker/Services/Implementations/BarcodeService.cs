using SewingTracker.Models;
using SewingTracker.Services.Interfaces;
using System.Drawing;
using System.Text;
using System.Text.Json;
using ZXing;
using ZXing.Common;

namespace SewingTracker.Services.Implementations
{
    public class BarcodeService : IBarcodeService
    {
        private readonly ZXing.Windows.Compatibility.BarcodeWriter barcodeWriter;

        public BarcodeService()
        {
            barcodeWriter = new ZXing.Windows.Compatibility.BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300,
                    Margin = 10
                }
            };
        }

        public string GenerateEmployeeBarcode(Employee employee)
        {
            var employeeData = new
            {
                Type = "EMPLOYEE",
                employee.Id,
                employee.Name,
                Timestamp = DateTime.UtcNow
            };

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(employeeData)));
        }

        public string GenerateReceiptBarcode(Cloth cloth)
        {
            var receiptData = new
            {
                Type = "RECEIPT",
                cloth.ClothId,
                cloth.Price,
                Date = cloth.ReceiptDate,
                cloth.Description,
                Timestamp = DateTime.UtcNow
            };

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(receiptData)));
        }

        public BarcodeData ParseBarcode(string barcodeData)
        {
            try
            {
                var decodedBytes = Convert.FromBase64String(barcodeData);
                var jsonString = Encoding.UTF8.GetString(decodedBytes);
                var data = JsonSerializer.Deserialize<dynamic>(jsonString);

                return new BarcodeData
                {
                    Type = data.GetProperty("Type").GetString(),
                    Data = jsonString,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch
            {
                throw new ArgumentException("Invalid barcode format");
            }
        }

        public Bitmap GenerateBarcodeImage(string data, int width = 300, int height = 100)
        {
            barcodeWriter.Options.Width = width;
            barcodeWriter.Options.Height = height;

            return barcodeWriter.Write(data);
        }
    }
}
