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
                Format = BarcodeFormat.EAN_13,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300,
                    Margin = 10,
                    PureBarcode = false, // Shows the number below the barcode
                }
            };
        }

        public string GenerateEmployeeBarcode(Employee employee)
        {
            // EAN-13 format: First 3 digits = company prefix (001 for employees)
            // Next 9 digits = employee ID (padded)
            // Last digit = check digit (calculated automatically)

            string companyPrefix = "001"; // Employee identifier
            string employeeId = employee.Id.ToString().PadLeft(9, '0');
            string barcodeWithoutChecksum = companyPrefix + employeeId;

            // Calculate and append check digit
            string completeBarcode = barcodeWithoutChecksum + CalculateEAN13CheckDigit(barcodeWithoutChecksum);

            return completeBarcode;
        }

        public string GenerateReceiptBarcode(Cloth cloth)
        {
            // EAN-13 format: First 3 digits = company prefix (002 for receipts)
            // Next 9 digits = cloth ID (converted from string or hash)
            // Last digit = check digit (calculated automatically)

            string companyPrefix = "002"; // Receipt identifier

            // Convert cloth ID to numeric value (using hash if needed)
            string numericClothId = ConvertToNumericId(cloth.Id.ToString(), 9);
            string barcodeWithoutChecksum = companyPrefix + numericClothId;

            // Calculate and append check digit
            string completeBarcode = barcodeWithoutChecksum + CalculateEAN13CheckDigit(barcodeWithoutChecksum);

            return completeBarcode;
        }

        public BarcodeData ParseBarcode(string barcodeData)
        {
            try
            {
                // Validate EAN-13 format
                if (string.IsNullOrWhiteSpace(barcodeData) || barcodeData.Length != 13)
                    throw new ArgumentException("Invalid EAN-13 barcode length");

                if (!barcodeData.All(char.IsDigit))
                    throw new ArgumentException("EAN-13 barcode must contain only digits");

                // Validate check digit
                string barcodeWithoutCheck = barcodeData.Substring(0, 12);
                string checkDigit = barcodeData.Substring(12, 1);

                if (checkDigit != CalculateEAN13CheckDigit(barcodeWithoutCheck))
                    throw new ArgumentException("Invalid EAN-13 check digit");

                // Parse barcode type based on prefix
                string prefix = barcodeData.Substring(0, 3);
                string type = prefix == "001" ? "EMPLOYEE" : prefix == "002" ? "RECEIPT" : "UNKNOWN";

                if (type == "UNKNOWN")
                    throw new ArgumentException("Unknown barcode prefix");

                return new BarcodeData
                {
                    Type = type,
                    Data = barcodeData,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid barcode format: {ex.Message}");
            }
        }

        public Bitmap GenerateBarcodeImage(string data, int width = 300, int height = 100)
        {
            // Validate EAN-13 format before generating
            if (data.Length != 13 || !data.All(char.IsDigit))
                throw new ArgumentException("Data must be a 13-digit EAN-13 barcode");

            barcodeWriter.Options.Width = width;
            barcodeWriter.Options.Height = height;

            return barcodeWriter.Write(data);
        }

        #region Helper methods
        // Helper method to calculate EAN-13 check digit
        private string CalculateEAN13CheckDigit(string barcode12)
        {
            if (barcode12.Length != 12)
                throw new ArgumentException("EAN-13 check digit calculation requires 12 digits");

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = int.Parse(barcode12[i].ToString());
                // Multiply odd positions (1-indexed) by 1, even positions by 3
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit.ToString();
        }

        // Helper method to convert string IDs to numeric format
        private string ConvertToNumericId(string input, int length)
        {
            // If input is already numeric and fits, use it
            if (input.All(char.IsDigit) && input.Length <= length)
                return input.PadLeft(length, '0');

            // Otherwise, create a numeric hash
            int hash = Math.Abs(input.GetHashCode());
            string numericId = hash.ToString();

            // Ensure it fits in the required length
            if (numericId.Length > length)
                numericId = numericId.Substring(0, length);
            else
                numericId = numericId.PadLeft(length, '0');

            return numericId;
        }
        #endregion Helper methods
    }
}
