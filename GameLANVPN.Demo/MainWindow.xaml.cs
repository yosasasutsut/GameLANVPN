using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameLANVPN.Demo
{
    public partial class MainWindow : Window
    {
        private string currentChannel = "";

        public MainWindow()
        {
            InitializeComponent();

            // Set DataContext for binding
            this.DataContext = LanguageManager.Instance;
        }

        private void JoinChannel_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.Name == "JoinGeneralBtn")
            {
                currentChannel = "Burning Hells";
                ChatChannelName.Text = "🔥 Burning Hells";
                ChatUserCount.Text = "487 damned souls";
            }
            else if (btn.Name == "JoinGamingBtn")
            {
                currentChannel = "Warriors' Sanctum";
                ChatChannelName.Text = "⚔️ Warriors' Sanctum";
                ChatUserCount.Text = "312 armed heroes";
            }

            // Show chat panel
            ChatPanel.Visibility = Visibility.Visible;

            // Add join message
            AddChatMessage("System", "You have joined " + currentChannel, "#FFD700");
        }

        private void LeaveChannel_Click(object sender, RoutedEventArgs e)
        {
            // Hide chat panel
            ChatPanel.Visibility = Visibility.Collapsed;
            currentChannel = "";
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendChatMessage();
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendChatMessage();
            }
        }

        private void SendChatMessage()
        {
            if (!string.IsNullOrWhiteSpace(ChatInput.Text))
            {
                AddChatMessage("DemonSlayer666", ChatInput.Text, "#FF4500");
                ChatInput.Text = "";
            }
        }

        private void AddChatMessage(string username, string message, string color)
        {
            var timeStamp = DateTime.Now.ToString("HH:mm");

            var messagePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            messagePanel.Children.Add(new TextBlock
            {
                Text = $"[{timeStamp}]",
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 0, 8, 0)
            });

            messagePanel.Children.Add(new TextBlock
            {
                Text = username + ":",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)),
                Margin = new Thickness(0, 0, 5, 0)
            });

            messagePanel.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 12,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0C0C0"))
            });

            ChatMessages.Children.Add(messagePanel);

            // Scroll to bottom
            if (ChatMessages.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }
        }


        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Close main window and show login window
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}