using GameLANVPN.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace GameLANVPN.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();

            // Allow window dragging
            MouseLeftButtonDown += (s, e) => {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}