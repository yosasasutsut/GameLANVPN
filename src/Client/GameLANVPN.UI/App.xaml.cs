using GameLANVPN.UI.Views;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace GameLANVPN.UI
{
    public partial class App : Application
    {
        private ILogger<App>? _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup global exception handling
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Initialize logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = loggerFactory.CreateLogger<App>();

            _logger?.LogInformation("GameLAN VPN Client starting...");

            // Show login window instead of main window
            var loginWindow = new LoginWindow();
            MainWindow = loginWindow;
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger?.LogInformation("GameLAN VPN Client shutting down...");
            base.OnExit(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            _logger?.LogError(ex, "Unhandled exception occurred");

            MessageBox.Show(
                $"An unexpected error occurred:\n{ex?.Message}",
                "GameLAN VPN Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Unhandled dispatcher exception occurred");

            MessageBox.Show(
                $"An UI error occurred:\n{e.Exception.Message}",
                "GameLAN VPN UI Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }
    }
}