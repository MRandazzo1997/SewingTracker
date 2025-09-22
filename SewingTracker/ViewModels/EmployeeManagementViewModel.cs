using Microsoft.EntityFrameworkCore;
using SewingTracker.Commands;
using SewingTracker.Data;
using SewingTracker.Models;
using SewingTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SewingTracker.ViewModels
{
    public class EmployeeManagementViewModel : BaseViewModel
    {
        private readonly TailorDbContext _context;
        private readonly IBarcodeService _barcodeService;

        private string _newEmployeeName;
        private Employee _selectedEmployee;
        private ObservableCollection<Employee> _employees;
        private string _statusMessage;
        private string _statusColor = "Black";
        private Bitmap _generatedBarcodeImage;

        public EmployeeManagementViewModel(TailorDbContext context, IBarcodeService barcodeService)
        {
            _context = context;
            _barcodeService = barcodeService;
            _employees = new ObservableCollection<Employee>();

            AddEmployeeCommand = new AsyncRelayCommand(AddEmployeeAsync, CanAddEmployee);
            LoadEmployeesCommand = new AsyncRelayCommand(LoadEmployeesAsync);
            GenerateBarcodeCommand = new AsyncRelayCommand(GenerateBarcodeAsync, CanGenerateBarcode);
            DeactivateEmployeeCommand = new AsyncRelayCommand(DeactivateEmployeeAsync, CanDeactivateEmployee);

            // Load employees on initialization
            Task.Run(() => LoadEmployeesAsync(null));
        }

        public string NewEmployeeName
        {
            get => _newEmployeeName;
            set
            {
                SetProperty(ref _newEmployeeName, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                SetProperty(ref _selectedEmployee, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
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

        public Bitmap GeneratedBarcodeImage
        {
            get => _generatedBarcodeImage;
            set => SetProperty(ref _generatedBarcodeImage, value);
        }

        public ICommand AddEmployeeCommand { get; }
        public ICommand LoadEmployeesCommand { get; }
        public ICommand GenerateBarcodeCommand { get; }
        public ICommand DeactivateEmployeeCommand { get; }

        private async Task AddEmployeeAsync(object parameter)
        {
            try
            {
                var employee = new Employee
                {
                    Name = NewEmployeeName.Trim(),
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Generate barcode after getting ID
                employee.EmployeeBarcode = _barcodeService.GenerateEmployeeBarcode(employee);
                await _context.SaveChangesAsync();

                Employees.Add(employee);
                NewEmployeeName = string.Empty;

                ShowSuccess($"Employee {employee.Name} added successfully");
            }
            catch (Exception ex)
            {
                ShowError($"Error adding employee: {ex.Message}");
            }
        }

        private bool CanAddEmployee(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NewEmployeeName);
        }

        private async Task LoadEmployeesAsync(object parameter)
        {
            try
            {
                var employees = await _context.Employees
                    .OrderByDescending(e => e.CreatedDate)
                    .ToListAsync();

                Employees.Clear();
                foreach (var emp in employees)
                {
                    Employees.Add(emp);
                }

                ShowSuccess($"Loaded {employees.Count} employees");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading employees: {ex.Message}");
            }
        }

        private async Task GenerateBarcodeAsync(object parameter)
        {
            if (SelectedEmployee == null) return;

            try
            {
                var barcodeData = SelectedEmployee.EmployeeBarcode;
                GeneratedBarcodeImage = _barcodeService.GenerateBarcodeImage(barcodeData);

                ShowSuccess($"Barcode generated for {SelectedEmployee.Name}");
            }
            catch (Exception ex)
            {
                ShowError($"Error generating barcode: {ex.Message}");
            }
        }

        private bool CanGenerateBarcode(object parameter)
        {
            return SelectedEmployee != null;
        }

        private async Task DeactivateEmployeeAsync(object parameter)
        {
            if (SelectedEmployee == null) return;

            try
            {
                SelectedEmployee.IsActive = false;
                await _context.SaveChangesAsync();

                ShowSuccess($"Employee {SelectedEmployee.Name} deactivated");
            }
            catch (Exception ex)
            {
                ShowError($"Error deactivating employee: {ex.Message}");
            }
        }

        private bool CanDeactivateEmployee(object parameter)
        {
            return SelectedEmployee != null && SelectedEmployee.IsActive;
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
