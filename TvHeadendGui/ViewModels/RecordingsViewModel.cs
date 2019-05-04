using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Prism.Commands;
using Prism.Regions;
using PropertyChanged;
using TvHeadendGui.Properties;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadendGui.ViewModels
{
	public class RecordingsViewModel : ViewModelBase
	{
        public ObservableCollection<Recording> Recordings { get; set; }
	    public Recording SelectedRecording { get; set; }

	    public DelegateCommand ReloadRecordings { get; set; }

        [AlsoNotifyFor(nameof(CanDeleteRecording))]
	    public DelegateCommand DeleteSelectedRecording { get; set; }

        [AlsoNotifyFor(nameof(CanDownloadRecording))]
	    public DelegateCommand DownloadSelectedRecording { get; set; }

	    public DelegateCommand CancelDownload { get; set; }

	    public int ProgressValue { get; set; }
	    public Visibility ProgressBarVisibility { get; set; }
	    public string StatusText { get; set; }

	    private bool DownloadIsCanceled { get; set; }

        public RecordingsViewModel(IRegionManager regionManager, ITvHeadend tvHeadend) : base(regionManager, tvHeadend)
        {
            Recordings = TvHeadend.Recordings;
            DeleteSelectedRecording = new DelegateCommand(OnDeleteSelected).ObservesCanExecute(()=>CanDeleteRecording);
            ReloadRecordings = new DelegateCommand(OnReloadRecordings);
            DownloadSelectedRecording = new DelegateCommand(OnDownloadSelectedRecording).ObservesCanExecute(()=>CanDownloadRecording);
            CancelDownload = new DelegateCommand(OnDownloadCanceled);
            StatusText = $"{Recordings.Count} Recordings.";
            ProgressBarVisibility = Visibility.Collapsed;
        }

	    private void OnDownloadCanceled()
	    {
	        DownloadIsCanceled = true;
	    }

	    private void OnReloadRecordings()
	    {
	        Recordings = TvHeadend.Recordings;
        }

	    public bool CanDownloadRecording => SelectedRecording?.Status == "Completed OK";
        private async void OnDownloadSelectedRecording()
        {
            var fileUrl = $"{TvHeadend.TvHeadendUri.AbsoluteUri}{SelectedRecording.Url}";
            var targetFileName = SelectedRecording.FileFullName.Substring(SelectedRecording.FileFullName.LastIndexOf("/")+1) ;

            var targetFilePath = Path.Combine(Settings.Default.VideoDownloadPath, targetFileName);

            using (var webClient = new WebClient {Credentials = TvHeadend.Credentials})
            {
                webClient.DownloadFileCompleted += Completed;
                webClient.DownloadProgressChanged += ProgressChanged;
                StatusText = "Downloading File:";
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
	        StatusText = $"Downloading File: {e.ProgressPercentage} %";
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

	    public bool CanDeleteRecording => SelectedRecording?.Status == "Scheduled for recording" || SelectedRecording?.Status == "Completed OK";
	    private void OnDeleteSelected()
	    {
            //ToDo: Use Mahapps Dialogs
            switch (SelectedRecording.Status)
            {
                case "Completed": //ToDo: vervollständigen
                    if (MessageBox.Show($"Do you really want to delete the recorded file {SelectedRecording.Title} from the server?", "Are you sure?", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                        TvHeadend.DeleteRecordedFile(SelectedRecording);
                    break;
                case "Scheduled for recording":
                    if (MessageBox.Show($"Do you really want to delete the recording schedule {SelectedRecording.Title}?", "Are you sure?", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                        TvHeadend.RemoveRecordingSchedule(SelectedRecording);
                    break;
            }
        }
    }
}
