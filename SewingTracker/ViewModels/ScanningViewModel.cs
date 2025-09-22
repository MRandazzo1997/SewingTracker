using SewingTracker.Commands;
using SewingTracker.Models;
using SewingTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SewingTracker.ViewModels
{
    public class ScanningViewModel : BaseViewModel
    {
        private readonly IScanningService _scanningService;

        private string _employeeBarcode;
        private string _receiptBarcode;
        private string _statusMessage;
        private Employee _scannedEmployee;
        private Cloth _scannedCloth;
        private bool _isProcessing;
        private string _statusColor = "Black";

        public ScanningViewModel(IScanningService scanningService)
        {
            _scanningService = scanningService;

            ScanEmployeeBarcodeCommand = new AsyncRelayCommand(ScanEmployeeBarcodeAsync);
            ScanReceiptBarcodeCommand = new AsyncRelayCommand(ScanReceiptBarcodeAsync);
            RegisterWorkCommand = new AsyncRelayCommand(RegisterWorkAsync, CanRegisterWork);
            ClearCommand = new RelayCommand(Clear);
        }

        public string EmployeeBarcode
        {
            get => _employeeBarcode;
            set => SetProperty(ref _employeeBarcode, value);
        }

        public string ReceiptBarcode
        {
            get => _receiptBarcode;
            set => SetProperty(ref _receiptBarcode, value);
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

        public Employee ScannedEmployee
        {
            get => _scannedEmployee;
            set
            {
                SetProperty(ref _scannedEmployee, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Cloth ScannedCloth
        {
            get => _scannedCloth;
            set
            {
                SetProperty(ref _scannedCloth, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ICommand ScanEmployeeBarcodeCommand { get; }
        public ICommand ScanReceiptBarcodeCommand { get; }
        public ICommand RegisterWorkCommand { get; }
        public ICommand ClearCommand { get; }

        private async Task ScanEmployeeBarcodeAsync(object parameter)
        {
            if (string.IsNullOrWhiteSpace(EmployeeBarcode))
            {
                ShowError("Please enter employee barcode");
                return;
            }

            IsProcessing = true;
            try
            {
                ScannedEmployee = await _scanningService.GetEmployeeFromBarcodeAsync(EmployeeBarcode);
                if (ScannedEmployee != null)
                {
                    ShowSuccess($"Employee: {ScannedEmployee.Name}");
                }
                else
                {
                    ShowError("Employee not found or inactive");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error scanning employee: {ex.Message}");
                ScannedEmployee = null;
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ScanReceiptBarcodeAsync(object parameter)
        {
            if (string.IsNullOrWhiteSpace(ReceiptBarcode))
            {
                ShowError("Please enter receipt barcode");
                return;
            }

            IsProcessing = true;
            try
            {
                ScannedCloth = await _scanningService.GetClothFromBarcodeAsync(ReceiptBarcode);
                if (ScannedCloth != null)
                {
                    ShowSuccess($"Cloth: {ScannedCloth.ClothId} - {ScannedCloth.Description}");
                }
                else
                {
                    ShowError("Cloth not found");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error scanning receipt: {ex.Message}");
                ScannedCloth = null;
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task RegisterWorkAsync(object parameter)
        {
            IsProcessing = true;
            try
            {
                var workRecord = await _scanningService.RegisterWorkCompletionAsync(EmployeeBarcode, ReceiptBarcode);
                ShowSuccess($"Work registered successfully! {ScannedEmployee.Name} completed work on {ScannedCloth.ClothId}");

                // Clear the form after successful registration
                Clear(null);
            }
            catch (Exception ex)
            {
                ShowError($"Error registering work: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool CanRegisterWork(object parameter)
        {
            return ScannedEmployee != null && ScannedCloth != null && !IsProcessing;
        }

        private void Clear(object parameter)
        {
            EmployeeBarcode = string.Empty;
            ReceiptBarcode = string.Empty;
            ScannedEmployee = null;
            ScannedCloth = null;
            StatusMessage = "Ready to scan";
            StatusColor = "Black";
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
