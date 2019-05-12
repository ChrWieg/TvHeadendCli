using System;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using Prism.Commands;
using System.Windows;
using System.Windows.Media;
using Prism.Events;
using Prism.Regions;
using PropertyChanged;
using TvHeadendGui.Events;
using TvHeadendGui.Properties;
using TvHeadendLib.Helper;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadendGui.ViewModels
{
    public class SettingsViewModel : ViewModelBase, INavigationAware
    {
        private string _statusText;
        private bool _settingsChanged;
        private const string TestOkayStatusText = "Rest client says:";
        private const string SettingsSavedStatusText = "Settings have been saved. Run test to prove";
        private const string SettingsResetStatusText = "Settings have been set to default values. Run test to prove.";

        private const string StandardStatusText = "Adjust settings for accessing your TvHeadend Server here.";
        private const string SettingsChangedStatusText = "Settings changed. Press the Save-button so save the new settings.";

        private const string TestFailedStatusText = "Test failed:";

        public DelegateCommand SaveChanges { get; set; }
        public DelegateCommand ResetToDefault { get; set; }
        public DelegateCommand TestSettings { get; set; }
        public DelegateCommand<string> CopyToClipBoard { get; set; }

        public string ServerName { get; set; }
        public int PortNumber { get; set; }
        public bool UseTls { get; set; }

        public bool AuthenticationRequired { get; set; }
        public bool SaveCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string VideoDownloadPath { get; set; }

        public string CreateParameterString { get; set; }

        public string RemoveParameterString { get; set; }

        [AlsoNotifyFor(nameof(StatusText))]
        public SolidColorBrush StatusTextColor { get; set; }

        [AlsoNotifyFor(nameof(OnSettingsChanged))]
        public bool SettingsChanged
        {
            get => _settingsChanged;
            set
            {
                SetProperty(ref _settingsChanged, value);
                if (_settingsChanged)
                    StatusText = SettingsChangedStatusText;
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                SetProperty(ref _statusText,value);

                if (_statusText == StandardStatusText || _statusText == SettingsChangedStatusText)
                    StatusTextColor = new SolidColorBrush(Colors.Orange);
                else if (_statusText.Contains(TestFailedStatusText))
                    StatusTextColor = new SolidColorBrush(Colors.Red);
                else
                    StatusTextColor = new SolidColorBrush(Colors.Green);

                EventAggregator.GetEvent<StatusChangedEvent>().Publish(new StatusInfo(_statusText));
            }
        }

        [AlsoNotifyFor(nameof(AuthenticationRequired))]
        public Visibility UnPwVisibility => AuthenticationRequired ? Visibility.Visible : Visibility.Collapsed;

        public SettingsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ITvHeadend tvHeadend) : base(regionManager, eventAggregator, tvHeadend)
        {
            SaveChanges = new DelegateCommand(OnSaveChanges).ObservesCanExecute( () => OnSettingsChanged);
            ResetToDefault = new DelegateCommand(OnResetToDefault);
            TestSettings = new DelegateCommand(OnTestSettings);
            CopyToClipBoard = new DelegateCommand<string>(Clipboard.SetText);
            PropertyChanged += (sender, args) =>
            {
                if (args is PropertyChangedEventArgs propertyChangedEventArgs 
                        && 
                        (
                            propertyChangedEventArgs.PropertyName == nameof(SettingsChanged) 
                            || 
                            propertyChangedEventArgs.PropertyName == nameof(OnSettingsChanged)
                            ||
                            propertyChangedEventArgs.PropertyName == nameof(StatusText)
                            ||
                            propertyChangedEventArgs.PropertyName == nameof(StatusTextColor)
                            )
                    )
                    return;

                SettingsChanged = true;
            };
        }

        public bool OnSettingsChanged => SettingsChanged;

        private void LoadSettingsData()
        {
            ServerName = Settings.Default.ServerName;
            PortNumber = Settings.Default.PortNumber;
            UseTls = Settings.Default.UseTls;
            
            AuthenticationRequired = Settings.Default.AuthenticationRequired;
            SaveCredentials = Settings.Default.SaveCredentialsToWindowsCredentialStore;

            VideoDownloadPath = Settings.Default.VideoDownloadPath;

            var credential = CredentialHelper.GetStoredCredential(false);
            if (credential != null)
            {
                UserName = credential.UserName;
                Password = credential.Password;
            }

            CreateParameterString = TvHeadend.GetCreateParameterString();
            RemoveParameterString = TvHeadend.GetRemoveParameterString();

            StatusText = StandardStatusText;
            SettingsChanged = false;
        }

        private void OnTestSettings()
        {
            if (SettingsChanged)
                OnSaveChanges();

            var testResult = TvHeadend.RestClientIsWorking();
            StatusText = $"{(testResult.ToLower().Contains("ok") ? TestOkayStatusText : TestFailedStatusText)} {testResult}";
        }

        private void OnResetToDefault()
        {
            if (MessageBox.Show("Do you really want to reset all settings zu default values?","Are you sure?",MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                Settings.Default.PortNumber = 9981;
                Settings.Default.ServerName = "TvHeadend";
                Settings.Default.UseTls = false;
                Settings.Default.AuthenticationRequired = true;
                Settings.Default.SaveCredentialsToWindowsCredentialStore = true;
                Settings.Default.VideoDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                Settings.Default.Save();
                Settings.Default.Reload();
                ServerName = Settings.Default.ServerName;
                PortNumber = Settings.Default.PortNumber;
            }

            OnSaveChanges();
            StatusText = SettingsResetStatusText;
            SettingsChanged = false;
        }

        private void OnSaveChanges()
        {
            Settings.Default.PortNumber = PortNumber;
            Settings.Default.ServerName = ServerName;
            Settings.Default.UseTls = UseTls;
            Settings.Default.AuthenticationRequired = AuthenticationRequired;
            Settings.Default.SaveCredentialsToWindowsCredentialStore = SaveCredentials;
            Settings.Default.VideoDownloadPath = VideoDownloadPath;

            if (AuthenticationRequired && SaveCredentials)
                CredentialHelper.ResetCredential(new NetworkCredential(UserName, Password));

            var protocol = UseTls ? "https://" : "http://";
            var url = $"{protocol}{ServerName}:{PortNumber}/";

            TvHeadend.TvHeadendBaseUri = new Uri(url);
            TvHeadend.Credentials = new Credential {Password = Password, UserName = UserName};

            Settings.Default.Save();
            Settings.Default.Reload();

            CreateParameterString = TvHeadend.GetCreateParameterString();
            RemoveParameterString = TvHeadend.GetRemoveParameterString();

            StatusText = SettingsSavedStatusText;
            SettingsChanged = false;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadSettingsData();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
