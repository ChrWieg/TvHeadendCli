using System.Collections.ObjectModel;
using System.Windows;
using Prism.Commands;
using Prism.Regions;
using PropertyChanged;
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

        public RecordingsViewModel(IRegionManager regionManager, ITvHeadend tvHeadend) : base(regionManager, tvHeadend)
        {
            Recordings = TvHeadend.Recordings;
            DeleteSelectedRecording = new DelegateCommand(OnDeleteSelected).ObservesCanExecute(()=>CanDeleteRecording);
            ReloadRecordings = new DelegateCommand(OnReloadRecordings);
            DownloadSelectedRecording = new DelegateCommand(OnDownloadSelectedRecording).ObservesCanExecute(()=>CanDownloadRecording);
        }

	    private void OnReloadRecordings()
	    {
	        Recordings = TvHeadend.Recordings;
        }

	    public bool CanDownloadRecording => SelectedRecording?.Status == "Completed OK";
        private void OnDownloadSelectedRecording()
        {
            MessageBox.Show(SelectedRecording.FileFullName);
        }

	    public bool CanDeleteRecording => SelectedRecording?.Status == "Scheduled for recording" || SelectedRecording?.Status == "Completed OK";
	    private void OnDeleteSelected()
	    {
            //ToDo: Mahapps Dialog
            if (MessageBox.Show($"Do you really want to delete recording {SelectedRecording.Title}?","Are you sure?",MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
	            TvHeadend.RemoveRecording(SelectedRecording);
	    }
	}
}
