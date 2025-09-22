using SewingTracker.Models;

namespace SewingTracker.Services.Interfaces
{
    public interface IScanningService
    {
        Task<Employee> GetEmployeeFromBarcodeAsync(string barcodeData);
        Task<Cloth> GetClothFromBarcodeAsync(string barcodeData);
        Task<WorkRecord> RegisterWorkCompletionAsync(string employeeBarcode, string receiptBarcode);
    }
}
