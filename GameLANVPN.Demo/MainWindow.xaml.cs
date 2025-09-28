using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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

            if (btn.Name == "SelectDotaAPBtn")
            {
                currentChannel = "DOTA All Pick";
                CurrentChannelTitle.Text = "⚔️ DOTA ALL PICK CHAT";
                CurrentChannelDesc.Text = "Choose any hero and dominate the battlefield";
            }
            else if (btn.Name == "SelectDotaEMBtn")
            {
                currentChannel = "DOTA Easy Mode";
                CurrentChannelTitle.Text = "⚡ DOTA EASY MODE CHAT";
                CurrentChannelDesc.Text = "Relaxed gameplay for beginners and casual players";
            }
            else if (btn.Name == "SelectDotaLODBtn")
            {
                currentChannel = "DOTA Legends of DotA";
                CurrentChannelTitle.Text = "🔮 LEGENDS OF DOTA CHAT";
                CurrentChannelDesc.Text = "Mix and match abilities for unique hero combinations";
            }
            else if (btn.Name == "SelectDotaIMBABtn")
            {
                currentChannel = "DOTA IMBA";
                CurrentChannelTitle.Text = "💥 DOTA IMBA CHAT";
                CurrentChannelDesc.Text = "Overpowered heroes and epic battles await";
            }
            else if (btn.Name == "SelectTowerDefenseBtn")
            {
                currentChannel = "Tower Defense";
                CurrentChannelTitle.Text = "🏯 TOWER DEFENSE CHAT";
                CurrentChannelDesc.Text = "Defend your base against endless waves of enemies";
            }

            // Hide channel selection page and show main UI
            ChannelSelectionPage.Visibility = Visibility.Collapsed;
            MainUIPanel.Visibility = Visibility.Visible;

            // Clear and add welcome message
            ChatMessages.Children.Clear();
            AddChatMessage("System", "Welcome to " + currentChannel + "!", "#FFD700");

            // Set default to Online tab
            OnlineTab_Click(null, null);
        }

        private void ChannelSelector_Click(object sender, RoutedEventArgs e)
        {
            // Back to channel selection page
            MainUIPanel.Visibility = Visibility.Collapsed;
            ChannelSelectionPage.Visibility = Visibility.Visible;
            currentChannel = "";
        }

        private void LeaveChannel_Click(object sender, RoutedEventArgs e)
        {
            // Back to channel selection
            MainUIPanel.Visibility = Visibility.Collapsed;
            ChannelSelectionPage.Visibility = Visibility.Visible;
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

        // Window control event handlers
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnlineTab_Click(object sender, RoutedEventArgs e)
        {
            // Clear current user list and show online users
            UserList.Children.Clear();

            // Add sample online users
            AddUserToList("WarriorKnight", "#00FF00");
            AddUserToList("MageSorcerer", "#00FF00");
            AddUserToList("RogueAssassin", "#FFFF00");
            AddUserToList("PaladinHoly", "#FF0000");
            AddUserToList("DarkWizard", "#00FF00");
            AddUserToList("ElfArcher", "#FFFF00");
        }

        private void FriendsTab_Click(object sender, RoutedEventArgs e)
        {
            // Clear current user list and show friends
            UserList.Children.Clear();

            // Add sample friends
            AddUserToList("BestFriend123", "#00FF00");
            AddUserToList("GuildMaster", "#00FF00");
            AddUserToList("TeamMate456", "#808080");
            AddUserToList("OldBuddy", "#FFFF00");
        }

        private void AddUserToList(string username, string statusColor)
        {
            var userPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            userPanel.Children.Add(new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(statusColor)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });

            userPanel.Children.Add(new TextBlock
            {
                Text = username,
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center
            });

            UserList.Children.Add(userPanel);
        }
    }
}