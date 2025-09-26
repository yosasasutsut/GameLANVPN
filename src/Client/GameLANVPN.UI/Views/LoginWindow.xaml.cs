using GameLANVPN.UI.ViewModels;
using System.Windows;

namespace GameLANVPN.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();

            // Focus on username field when window loads
            Loaded += (s, e) => {
                if (string.IsNullOrEmpty(((LoginViewModel)DataContext).ServerUrl))
                {
                    ServerUrlTextBox.Focus();
                }
                else
                {
                    UsernameTextBox.Focus();
                }
            };

            // Handle successful login
            ((LoginViewModel)DataContext).LoginSuccessful += OnLoginSuccessful;
        }

        private void OnLoginSuccessful()
        {
            // Hide login window and show main window
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            this.Close();
        }
    }
}