using SewingTracker.Commands;
using SewingTracker.Models;
using SewingTracker.Services.Interfaces;
using System.Drawing;
using System.Windows.Input;

namespace SewingTracker.ViewModels
{
    public class ReceiptRegistrationViewModel : BaseViewModel
    {
        private readonly IReceiptService _receiptService;
        private readonly IBarcodeService _barcodeService;

        private string _clothId;
        private string _description;
        private string _priceText;
        private string _customerInfo;
        private string _statusMessage;
        private string _statusColor = "Black";
        private Bitmap _generatedBarcode;
        private Cloth _lastRegisteredCloth;

        public ReceiptRegistrationViewModel(IReceiptService receiptService, IBarcodeService barcodeService)
        {
            _receiptService = receiptService;
            _barcodeService = barcodeService;

            RegisterReceiptCommand = new AsyncRelayCommand(RegisterReceiptAsync, CanRegisterReceipt);
            ClearCommand = new RelayCommand(Clear);
            PrintBarcodeCommand = new RelayCommand(PrintBarcode, CanPrintBarcode);
        }

        public string ClothId
        {
            get => _clothId;
            set
            {
                SetProperty(ref _clothId, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string PriceText
        {
            get => _priceText;
            set
            {
                SetProperty(ref _priceText, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CustomerInfo
        {
            get => _customerInfo;
            set => SetProperty(ref _customerInfo, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public Bitmap GeneratedBarcode
        {
            get => _generatedBarcode;
            set => SetProperty(ref _generatedBarcode, value);
        }

        public Cloth LastRegisteredCloth
        {
            get => _lastRegisteredCloth;
            set
            {
                SetProperty(ref _lastRegisteredCloth, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand RegisterReceiptCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand PrintBarcodeCommand { get; }

        private async Task RegisterReceiptAsync(object parameter)
        {
            try
            {
                if (!decimal.TryParse(PriceText, out decimal price))
                {
                    ShowError("Invalid price format");
                    return;
                }

                var cloth = await _receiptService.RegisterNewReceiptAsync(
                    ClothId.Trim(),
                    Description.Trim(),
                    price,
                    CustomerInfo?.Trim()
                );

                LastRegisteredCloth = cloth;

                // Generate barcode image
                GeneratedBarcode = _barcodeService.GenerateBarcodeImage(cloth.ReceiptBarcode, 400, 150);

                ShowSuccess($"Receipt registered successfully! Barcode: {cloth.ReceiptBarcode}");
            }
            catch (Exception ex)
            {
                ShowError($"Error registering receipt: {ex.Message}");
            }
        }

        private bool CanRegisterReceipt(object parameter)
        {
            return !string.IsNullOrWhiteSpace(ClothId) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   !string.IsNullOrWhiteSpace(PriceText);
        }

        private void Clear(object parameter)
        {
            ClothId = string.Empty;
            Description = string.Empty;
            PriceText = string.Empty;
            CustomerInfo = string.Empty;
            GeneratedBarcode = null;
            LastRegisteredCloth = null;
            StatusMessage = "Ready to register new receipt";
            StatusColor = "Black";
        }

        private void PrintBarcode(object parameter)
        {
            if (GeneratedBarcode == null) return;

            try
            {
                // This is a basic implementation
                // You can enhance this with proper print dialog and settings
                var printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawImage(GeneratedBarcode, 100, 100);
                };
                printDoc.Print();

                ShowSuccess("Barcode sent to printer");
            }
            catch (Exception ex)
            {
                ShowError($"Error printing: {ex.Message}");
            }
        }

        private bool CanPrintBarcode(object parameter)
        {
            return GeneratedBarcode != null;
        }

        private void ShowSuccess(string message)
        {
            StatusMessage = message;
            StatusColor = "Green";
        }

        private void ShowError(string message)
        {
            StatusMessage = message;
            StatusColor = "Red";
        }
    }
}
