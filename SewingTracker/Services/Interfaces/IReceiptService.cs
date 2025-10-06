using SewingTracker.Models;

namespace SewingTracker.Services.Interfaces
{
    public interface IReceiptService
    {
        Task<Cloth> RegisterNewReceiptAsync(string clothId, string description, decimal price, string customerInfo = null);
        Task<Cloth> GetClothByBarcodeAsync(string barcode);
        Task<bool> IsReceiptRegisteredAsync(string barcode);
    }
}
