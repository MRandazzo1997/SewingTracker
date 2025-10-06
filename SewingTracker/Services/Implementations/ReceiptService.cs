using Microsoft.EntityFrameworkCore;
using SewingTracker.Data;
using SewingTracker.Models;
using SewingTracker.Services.Interfaces;

namespace SewingTracker.Services.Implementations
{
    public class ReceiptService : IReceiptService
    {
        private readonly TailorDbContext _context;
        private readonly IBarcodeService _barcodeService;

        public ReceiptService(TailorDbContext context, IBarcodeService barcodeService)
        {
            _context = context;
            _barcodeService = barcodeService;
        }

        public async Task<Cloth> RegisterNewReceiptAsync(string clothId, string description, decimal price, string customerInfo = null)
        {
            // Check if cloth ID already exists
            var existing = await _context.Cloths.FirstOrDefaultAsync(c => c.ClothId == clothId);
            if (existing != null)
                throw new InvalidOperationException($"Cloth ID '{clothId}' already exists");

            // Create new cloth record
            var cloth = new Cloth
            {
                ClothId = clothId,
                Description = description,
                Price = price,
                ReceiptDate = DateTime.Now,
                CustomerInfo = customerInfo
            };

            _context.Cloths.Add(cloth);
            await _context.SaveChangesAsync();

            // Generate barcode after getting database ID
            cloth.ReceiptBarcode = _barcodeService.GenerateReceiptBarcode(cloth);
            await _context.SaveChangesAsync();

            return cloth;
        }

        public async Task<Cloth> GetClothByBarcodeAsync(string barcode)
        {
            return await _context.Cloths
                .FirstOrDefaultAsync(c => c.ReceiptBarcode == barcode);
        }

        public async Task<bool> IsReceiptRegisteredAsync(string barcode)
        {
            return await _context.Cloths.AnyAsync(c => c.ReceiptBarcode == barcode);
        }
    }
}
