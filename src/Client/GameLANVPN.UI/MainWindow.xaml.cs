using GameLANVPN.UI.ViewModels;
using GameLANVPN.Core.Network;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GameLANVPN.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Set DataContext
            DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Core services
            services.AddSingleton<INetworkManager, NetworkManager>();
            services.AddSingleton<MainWindowViewModel>();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Cleanup
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Dispose resources if needed
            }

            Application.Current.Shutdown();
        }
    }
}