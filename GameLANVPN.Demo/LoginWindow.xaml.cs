using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace GameLANVPN.Demo
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Set DataContext for binding
            this.DataContext = LanguageManager.Instance;

            // Subscribe to language changes
            LanguageManager.Instance.PropertyChanged += (s, e) => UpdateUI();

            // Allow window dragging
            MouseLeftButtonDown += (s, e) => {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            // Set focus to password if username is already filled
            Loaded += (s, e) => {
                if (!string.IsNullOrEmpty(UsernameTextBox.Text))
                {
                    PasswordBox.Focus();
                }
                else
                {
                    UsernameTextBox.Focus();
                }
            };

            // Enter key handling
            UsernameTextBox.KeyDown += (s, e) => {
                if (e.Key == Key.Enter)
                {
                    PasswordBox.Focus();
                }
            };

            PasswordBox.KeyDown += (s, e) => {
                if (e.Key == Key.Enter)
                {
                    LoginButton_Click(this, new RoutedEventArgs());
                }
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StatusMessage.Text = "🔥 MouseDown detected! Processing login...";
            StatusMessage.Foreground = System.Windows.Media.Brushes.Orange;
            LoginButton_Click(sender, new RoutedEventArgs());
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Debug info - Show that button was clicked
            StatusMessage.Text = $"🔍 Button clicked! Username='{username}', Password='{password}'";
            StatusMessage.Foreground = System.Windows.Media.Brushes.Yellow;

            // Simple test credentials
            if (username.ToLower() == "test" && password == "test")
            {
                StatusMessage.Text = LanguageManager.Instance.LoginSuccess;
                StatusMessage.Foreground = System.Windows.Media.Brushes.Gold;

                // Simulate login delay
                System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ => {
                    Dispatcher.Invoke(() => {
                        try
                        {
                            StatusMessage.Text = "🔄 Creating MainWindow...";
                            StatusMessage.Foreground = System.Windows.Media.Brushes.Cyan;

                            // Open main window
                            MainWindow mainWindow = new MainWindow();
                            StatusMessage.Text = "✅ MainWindow created, showing...";
                            mainWindow.Show();

                            StatusMessage.Text = "🚪 Closing login window...";
                            // Close login window
                            this.Close();
                        }
                        catch (System.Exception ex)
                        {
                            StatusMessage.Text = $"❌ Error: {ex.Message}";
                            StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
                        }
                    });
                });
            }
            else
            {
                StatusMessage.Text = LanguageManager.Instance.LoginFailed;
                StatusMessage.Foreground = System.Windows.Media.Brushes.Red;

                // Clear password
                PasswordBox.Password = "";
                PasswordBox.Focus();
            }
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string languageCode = selectedItem.Tag.ToString();
                LanguageManager.Instance.SetLanguage(languageCode);
            }
        }

        private void UpdateUI()
        {
            // This will be called when language changes
            // The binding will automatically update most text
            // Update any hardcoded text here if needed
        }
    }
}