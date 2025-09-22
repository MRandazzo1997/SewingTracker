using SewingTracker.Models;
using System.Drawing;

namespace SewingTracker.Services.Interfaces
{
    public interface IBarcodeService
    {
        string GenerateEmployeeBarcode(Employee employee);
        string GenerateReceiptBarcode(Cloth cloth);
        BarcodeData ParseBarcode(string barcodeData);
        Bitmap GenerateBarcodeImage(string data, int width = 300, int height = 100);
    }
}
