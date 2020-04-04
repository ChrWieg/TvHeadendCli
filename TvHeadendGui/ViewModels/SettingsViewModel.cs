using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Prism.Commands;
using System.Windows;
using System.Windows.Media;
using Prism.Events;
using Prism.Regions;
using PropertyChanged;
using TvHeadendGui.Events;
using TvHeadendGui.Helper;
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
        private const string TestOkayStatusText = "Server says:";
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
        public string ServerPath { get; set; }
        public int PortNumber { get; set; }
        public bool UseTls { get; set; }

        public bool AuthenticationRequired { get; set; }
        public bool SaveCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string VideoDownloadPath { get; set; }

        public string CreateParameterString { get; set; }

        public string RemoveParameterString { get; set; }
        public string DotNetVersion { get; set; }
        public string TvHeadendVersion { get; set; }

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
            CopyToClipBoard = new DelegateCommand<string>(OnCopyToClipboard);
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
            DotNetVersion = GetDotNetVersion.Get45PlusFromRegistry();
            TvHeadendVersion = tvHeadend.GetTvHeadendVersion();
        }

        private void OnCopyToClipboard(string content)
        {
            StatusText = StandardStatusText;

            var thread = new Thread(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        Clipboard.SetText(content);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusText = "String copied to Clipboard";
                        });

                        return;
                    }
                    catch (Exception)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusText = "Clipboard is used by another Application, trying again..";
                            StatusTextColor = new SolidColorBrush(Colors.Orange);
                        });

                        Thread.Sleep(100);
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusText = "Clipboard is used by another Application: sorry, I have given up. Please try again after a while.";
                    StatusTextColor = new SolidColorBrush(Colors.Red);
                });
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
        }


        public bool OnSettingsChanged => SettingsChanged;

        private void LoadSettingsData()
        {

            ServerName = Settings.Default.ServerName;
            ServerPath = Settings.Default.ServerPath;
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

            var testResult = TvHeadend.GetRestClientIsWorking();
            StatusText = $"{(testResult.Contains("Okay") ? TestOkayStatusText : TestFailedStatusText)} {testResult}";
        }

        private void OnResetToDefault()
        {
            if (MessageBox.Show("Do you really want to reset all settings zu default values?","Are you sure?",MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                Settings.Default.PortNumber = 9981;
                Settings.Default.ServerName = "TvHeadend";
                Settings.Default.ServerPath = "/";
                Settings.Default.UseTls = false;
                Settings.Default.AuthenticationRequired = true;
                Settings.Default.SaveCredentialsToWindowsCredentialStore = true;
                Settings.Default.VideoDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                Settings.Default.Save();
                Settings.Default.Reload();
                ServerName = Settings.Default.ServerName;
                ServerPath = Settings.Default.ServerPath;
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
            Settings.Default.ServerPath = ServerPath;
            Settings.Default.UseTls = UseTls;
            Settings.Default.AuthenticationRequired = AuthenticationRequired;
            Settings.Default.SaveCredentialsToWindowsCredentialStore = SaveCredentials;
            Settings.Default.VideoDownloadPath = VideoDownloadPath;

            if (AuthenticationRequired && SaveCredentials)
                CredentialHelper.ResetCredential(new NetworkCredential(UserName, Password));

            var uriBuilder = new UriBuilder
            {
                Host = ServerName, 
                Path = ServerPath, 
                Port = PortNumber, 
                Scheme = UseTls ? "https" : "http"
            };

            TvHeadend.TvHeadendBaseUri = uriBuilder.Uri;
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
