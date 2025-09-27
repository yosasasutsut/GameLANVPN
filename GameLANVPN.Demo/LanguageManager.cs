using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace GameLANVPN.Demo
{
    public class LanguageManager : INotifyPropertyChanged
    {
        private static LanguageManager _instance;
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public static LanguageManager Instance => _instance ??= new LanguageManager();

        public event PropertyChangedEventHandler PropertyChanged;

        private LanguageManager()
        {
            _resourceManager = new ResourceManager("GameLANVPN.Demo.Resources.Strings", typeof(LanguageManager).Assembly);
            // Set Thai as default language
            _currentCulture = new CultureInfo("th-TH");
            Thread.CurrentThread.CurrentCulture = _currentCulture;
            Thread.CurrentThread.CurrentUICulture = _currentCulture;
        }

        public string GetString(string key)
        {
            try
            {
                return _resourceManager.GetString(key, _currentCulture) ?? key;
            }
            catch
            {
                return key;
            }
        }

        public void SetLanguage(string languageCode)
        {
            _currentCulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = _currentCulture;
            Thread.CurrentThread.CurrentUICulture = _currentCulture;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        public string CurrentLanguage => _currentCulture.Name;

        // Properties for binding
        public string LoginTitle => GetString("LoginTitle");
        public string LoginSubtitle => GetString("LoginSubtitle");
        public string UsernameLabel => GetString("UsernameLabel");
        public string PasswordLabel => GetString("PasswordLabel");
        public string LoginButton => GetString("LoginButton");
        public string LoginSuccess => GetString("LoginSuccess");
        public string LoginFailed => GetString("LoginFailed");
        public string FooterText => GetString("FooterText");

        public string MainTitle => GetString("MainTitle");
        public string MainSubtitle => GetString("MainSubtitle");
        public string ForgeChannelButton => GetString("ForgeChannelButton");
        public string RefreshButton => GetString("RefreshButton");
        public string LogoutButton => GetString("LogoutButton");

        public string BurningHellsTitle => GetString("BurningHellsTitle");
        public string BurningHellsDesc => GetString("BurningHellsDesc");
        public string JoinHellButton => GetString("JoinHellButton");
        public string WarriorsSanctumTitle => GetString("WarriorsSanctumTitle");
        public string WarriorsSanctumDesc => GetString("WarriorsSanctumDesc");
        public string JoinBattleButton => GetString("JoinBattleButton");

        public string LeaveChannelButton => GetString("LeaveChannelButton");
        public string ChatPlaceholder => GetString("ChatPlaceholder");
        public string SendButton => GetString("SendButton");
        public string JoinedChannel => GetString("JoinedChannel");

        public string DarkChampionTitle => GetString("DarkChampionTitle");
        public string EnhancePowerButton => GetString("EnhancePowerButton");
        public string InfernalNexusTitle => GetString("InfernalNexusTitle");
        public string HellPortalDelay => GetString("HellPortalDelay");
        public string SoulTransmission => GetString("SoulTransmission");
        public string DarkEnergyFlow => GetString("DarkEnergyFlow");
        public string DemonIP => GetString("DemonIP");
        public string AwaitingSummon => GetString("AwaitingSummon");
        public string CursedArsenalTitle => GetString("CursedArsenalTitle");
        public string SeekDarkPowersButton => GetString("SeekDarkPowersButton");

        public string FooterMainText => GetString("FooterMainText");
        public string DamnedSouls => GetString("DamnedSouls");

        public string LanguageSelector => GetString("LanguageSelector");
        public string Thai => GetString("Thai");
        public string English => GetString("English");
    }
}