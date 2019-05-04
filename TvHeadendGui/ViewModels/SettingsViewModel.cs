using System;
using System.IO;
using System.Net;
using Prism.Commands;
using System.Windows;
using Prism.Regions;
using PropertyChanged;
using TvHeadendGui.Properties;
using TvHeadendLib.Helper;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public DelegateCommand SaveChanges { get; set; }
        public DelegateCommand ResetToDefault { get; set; }
        public DelegateCommand TestSettings { get; set; }
        public DelegateCommand<string> CopyToClipBoard { get; set; }

        public string ServerName { get; set; }
        public int PortNumber { get; set; }

        public bool AuthenticationRequired { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string VideoDownloadPath { get; set; }

        public string CreateParameterString { get; set; }

        public string RemoveParameterString { get; set; }

        public int ProgressValue { get; set; }
        public Visibility ProgressBarVisibility { get; set; }
        public string StatusText { get; set; }

        [AlsoNotifyFor(nameof(AuthenticationRequired))]
        public Visibility UnPwVisibility => AuthenticationRequired ? Visibility.Visible : Visibility.Collapsed;

        public SettingsViewModel(IRegionManager regionManager, ITvHeadend tvHeadend) : base(regionManager, tvHeadend)
        {
            ProgressBarVisibility = Visibility.Collapsed;
            SaveChanges = new DelegateCommand(OnSaveChanges);
            ResetToDefault = new DelegateCommand(OnResetToDefault);
            TestSettings = new DelegateCommand(OnTestSettings);
            CopyToClipBoard = new DelegateCommand<string>(Clipboard.SetText);

            ServerName = Settings.Default.ServerName;
            PortNumber = Settings.Default.PortNumber;
            AuthenticationRequired = Settings.Default.AuthenticationRequired;

            VideoDownloadPath = Settings.Default.VideoDownloadPath;

            var credential = CredentialHelper.GetStoredCredential(false);
            if (credential != null)
            {
                UserName = credential.UserName;
                Password = credential.Password;
            }

            CreateParameterString = TvHeadend.GetCreateParameterString();
            RemoveParameterString = TvHeadend.GetRemoveParameterString();

            StatusText = "Done.";
        }

        private void OnTestSettings()
        {
            StatusText = TvHeadend.RestClientIsOkay();
        }

        private void OnResetToDefault()
        {
            if (MessageBox.Show("Do you really want to reset all settings zu default values?","Are you sure?",MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                Settings.Default.PortNumber = 9981;
                Settings.Default.ServerName = "TvHeadend";
                Settings.Default.AuthenticationRequired = true;
                Settings.Default.VideoDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                Settings.Default.Save();
                Settings.Default.Reload();
                ServerName = Settings.Default.ServerName;
                PortNumber = Settings.Default.PortNumber;
            }
        }

        private void OnSaveChanges()
        {
            Settings.Default.PortNumber = PortNumber;
            Settings.Default.ServerName = ServerName;
            Settings.Default.AuthenticationRequired = AuthenticationRequired;
            Settings.Default.VideoDownloadPath = VideoDownloadPath;
            if (AuthenticationRequired)
            {
                CredentialHelper.ResetCredential(new NetworkCredential(UserName,Password));
            }

            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
