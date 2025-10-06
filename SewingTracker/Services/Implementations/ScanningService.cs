using SewingTracker.Models;
using SewingTracker.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SewingTracker.Services.Interfaces;

namespace SewingTracker.Services.Implementations
{
    public class ScanningService : IScanningService
    {
        private readonly TailorDbContext _context;
        private readonly IBarcodeService _barcodeService;

        public ScanningService(TailorDbContext context, IBarcodeService barcodeService)
        {
            _context = context;
            _barcodeService = barcodeService;
        }

        public async Task<Employee> GetEmployeeFromBarcodeAsync(string barcodeData)
        {
            try
            {
                var parsedData = _barcodeService.ParseBarcode(barcodeData);

                if (parsedData.Type != "EMPLOYEE")
                    throw new ArgumentException("Invalid employee barcode");

                // Extract employee ID from EAN-13 barcode
                // Format: 001 + 9-digit employee ID + check digit
                string employeeIdStr = barcodeData.Substring(3, 9);
                int employeeId = int.Parse(employeeIdStr);

                return await _context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);
            }
            catch
            {
                throw new ArgumentException("Unable to parse employee barcode");
            }
        }

        // Fix for CS1963: An expression tree may not contain a dynamic operation
        // The issue arises because `dynamic` types cannot be used in LINQ expressions that are translated to SQL.
        // To fix this, we need to cast the dynamic object to a concrete type or extract the value outside the LINQ query.

        public async Task<Cloth> GetClothFromBarcodeAsync(string barcodeData)
        {
            try
            {
                var parsedData = _barcodeService.ParseBarcode(barcodeData);

                if (parsedData.Type != "RECEIPT")
                    throw new ArgumentException("Invalid receipt barcode");

                // Use the extracted value in the LINQ query
                var existingCloth = await _context.Cloths
                    .FirstOrDefaultAsync(c => c.ReceiptBarcode == barcodeData);

                if (existingCloth != null)
                    return existingCloth;

                // Create new cloth record if not exists
                //var newCloth = new Cloth
                //{
                //    ClothId = clothId,
                //    Description = receiptData["Description"]?.ToString(),
                //    Price = Convert.ToDecimal(receiptData["Price"]),
                //    ReceiptDate = DateTime.Parse(receiptData["Date"]?.ToString()),
                //    ReceiptBarcode = barcodeData
                //};

                //_context.Cloths.Add(newCloth);
                //await _context.SaveChangesAsync();

                //return newCloth;
                throw new ArgumentException("Cloth not found. Please register this receipt first.");
            }
            catch
            {
                throw new ArgumentException("Unable to parse receipt barcode");
            }
        }

        public async Task<WorkRecord> RegisterWorkCompletionAsync(string employeeBarcode, string receiptBarcode)
        {
            var employee = await GetEmployeeFromBarcodeAsync(employeeBarcode);
            var cloth = await GetClothFromBarcodeAsync(receiptBarcode);

            if (employee == null)
                throw new ArgumentException("Employee not found or inactive");

            if (cloth == null)
                throw new ArgumentException("Cloth not found");

            // Check if work already registered
            var existingRecord = await _context.WorkRecords
                .FirstOrDefaultAsync(w => w.EmployeeId == employee.Id && w.ClothId == cloth.Id);

            if (existingRecord != null)
                throw new InvalidOperationException("Work already registered for this cloth and employee");

            var workRecord = new WorkRecord
            {
                EmployeeId = employee.Id,
                ClothId = cloth.Id,
                CompletedDate = DateTime.Now,
                Employee = employee,
                Cloth = cloth
            };

            _context.WorkRecords.Add(workRecord);
            await _context.SaveChangesAsync();

            return workRecord;
        }
    }
}
