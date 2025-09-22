using Microsoft.Extensions.DependencyInjection;
using SewingTracker.Data;
using SewingTracker.Services.Implementations;
using SewingTracker.Services.Interfaces;
using SewingTracker.ViewModels;
using System.Windows;

namespace SewingTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow()
        {
            InitializeComponent();

            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize database
            InitializeDatabase();

            // Set up ViewModels
            SetupViewModels();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register DbContext
            services.AddDbContext<TailorDbContext>();

            // Register services
            services.AddTransient<IBarcodeService, BarcodeService>();
            services.AddTransient<IScanningService, ScanningService>();

            // Register ViewModels
            services.AddTransient<ScanningViewModel>();
            services.AddTransient<EmployeeManagementViewModel>();
        }

        private void InitializeDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TailorDbContext>();
            context.Database.EnsureCreated();
        }

        private void SetupViewModels()
        {
            // Get the main tab control
            var tabControl = FindName("MainTabControl") as System.Windows.Controls.TabControl;

            if (tabControl != null)
            {
                // Set DataContext for Work Scanning tab
                var scanningTab = tabControl.Items[0] as System.Windows.Controls.TabItem;
                if (scanningTab != null)
                {
                    scanningTab.DataContext = _serviceProvider.GetRequiredService<ScanningViewModel>();
                }

                // Set DataContext for Employee Management tab
                var employeeTab = tabControl.Items[1] as System.Windows.Controls.TabItem;
                if (employeeTab != null)
                {
                    employeeTab.DataContext = _serviceProvider.GetRequiredService<EmployeeManagementViewModel>();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_serviceProvider is IDisposable disposableServiceProvider)
            {
                disposableServiceProvider.Dispose();
            }
            base.OnClosed(e);
        }
    }
}