using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Prism.Commands;
using Prism.Common;
using Prism.Events;
using Prism.Regions;
using PropertyChanged;
using TvHeadendLib.Interfaces;
using Settings = TvHeadendGui.Properties.Settings;

namespace TvHeadendGui.ViewModels
{
    public class RecordingViewModel : DependencyObjectBase, INavigationAware
    {
        [AlsoNotifyFor(nameof(ControlVisibility))]
        public TvHeadendLib.Models.Recording Recording { get; set; }

        public Visibility ProgressBarVisibility { get; set; }
        public int ProgressValue { get; set; }
        public string StatusText { get; set; }

        public Visibility ControlVisibility => Recording != null ? Visibility.Visible : Visibility.Collapsed;

        [AlsoNotifyFor(nameof(CanDelete))]
        public DelegateCommand DeleteSelectedRecording { get; set; }

        [AlsoNotifyFor(nameof(CanDownload))]
        public DelegateCommand DownloadSelectedRecording { get; set; }

        public DelegateCommand CancelDownload { get; set; }

        private bool DownloadIsCanceled { get; set; }

        public RecordingViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ITvHeadend tvHeadend) : base(regionManager, eventAggregator, tvHeadend)
        {
            DeleteSelectedRecording = new DelegateCommand(OnDelete).ObservesCanExecute(() => CanDelete);
            DownloadSelectedRecording = new DelegateCommand(OnDownload).ObservesCanExecute(() => CanDownload);
            CancelDownload = new DelegateCommand(OnDownloadCanceled);
            ProgressBarVisibility = Visibility.Collapsed;

            var viewRegionContext = RegionContext.GetObservableContext(this);
            Recording = (TvHeadendLib.Models.Recording)viewRegionContext.Value;
            viewRegionContext.PropertyChanged += ViewRegionContextOnPropertyChangedEvent;
        }

        private void ViewRegionContextOnPropertyChangedEvent(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Value")
            {
                var context = (ObservableObject<object>)sender;
                Recording = (TvHeadendLib.Models.Recording)context.Value;
            }
        }

        private void OnDownloadCanceled()
        {
            DownloadIsCanceled = true;
        }

        public bool CanDownload => Recording?.Status == "Completed OK";

        private async void OnDownload()
        {
            var fileUrl = $"{TvHeadend.TvHeadendBaseUri.AbsoluteUri}{Recording.Url}";
            var targetFileName = Recording.FileFullName.Substring(Recording.FileFullName.LastIndexOf("/") + 1);

            var targetFilePath = Path.Combine(Settings.Default.VideoDownloadPath, targetFileName);

            var credentials = new NetworkCredential(TvHeadend.Credentials.UserName, TvHeadend.Credentials.Password);
            using (var webClient = new WebClient { Credentials = credentials })
            {
                webClient.DownloadFileCompleted += Completed;
                webClient.DownloadProgressChanged += ProgressChanged;
                ProgressBarVisibility = Visibility.Visible;

                try
                {
                    await webClient.DownloadFileTaskAsync(new Uri(fileUrl), targetFilePath);
                }
                catch (WebException wex)
                {
                    ProgressValue = 100;
                    ProgressBarVisibility = Visibility.Collapsed;
                    StatusText = wex.Message;
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (DownloadIsCanceled && sender is WebClient webClient)
            {
                webClient.CancelAsync();
                return;
            }

            ProgressValue = e.ProgressPercentage;
            StatusText = $"Downloading Video: {e.ProgressPercentage} %";
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                ProgressValue = 100;
                ProgressBarVisibility = Visibility.Collapsed;
                StatusText = "Download canceled!";
                return;
            }

            ProgressValue = 100;
            ProgressBarVisibility = Visibility.Collapsed;
            StatusText = "Download completed!";
            Process.Start(Settings.Default.VideoDownloadPath);
        }

        public bool CanDelete =>
            Recording?.Status == "Scheduled for recording" ||
            Recording?.Status == "Completed OK" ||
            Recording?.Status == "Not enough disk space";

        private void OnDelete()
        {
            //ToDo: Use Mahapps Dialogs
            //ToDo: complete list
            switch (Recording.Status)
            {
                case "Completed OK":
                    if (MessageBox.Show($"Do you really want to delete the recorded file {Recording.Title} from the server?", "Are you sure?", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                        TvHeadend.DeleteRecordedFile(Recording);
                    break;
                case "Scheduled for recording":
                    if (MessageBox.Show($"Do you really want to delete the recording schedule {Recording.Title}?", "Are you sure?", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                        TvHeadend.RemoveRecordingSchedule(Recording);
                    break;
                case "Not enough disk space":
                    TvHeadend.DeleteRecordedFile(Recording);
                    break;
            }

            Recording = null;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["DataContext"] is TvHeadendLib.Models.Recording recording)
                Recording = recording;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            if (Recording == null)
                return true;

            if (navigationContext.Parameters["DataContext"] is TvHeadendLib.Models.Recording recording)
                return recording.Channel == Recording.Channel && recording.Start == Recording.Start;

            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
    }
}
