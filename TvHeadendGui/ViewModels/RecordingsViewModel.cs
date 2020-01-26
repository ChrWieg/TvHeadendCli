using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using PropertyChanged;
using TvHeadendGui.Events;
using TvHeadendGui.Helper;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadendGui.ViewModels
{
	public class RecordingsViewModel : ViewModelBase, INavigationAware
    {
        [AlsoNotifyFor(nameof(NoRecordingsFoundLabelVisibility))]
        public ObservableCollection<Recording> Recordings { get; set; }

        public Visibility NoRecordingsFoundLabelVisibility => Recordings?.Count < 1 ? Visibility.Visible : Visibility.Collapsed;

	    public DelegateCommand ReloadRecordings { get; set; }

	    public RecordingsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ITvHeadend tvHeadend) : base(regionManager, eventAggregator, tvHeadend)
	    {
            Recordings = new ObservableCollection<Recording>();

            ReloadRecordings = new DelegateCommand(OnLoadRecordings);

            EventAggregator.GetEvent<RecordingChangedEvent>().Subscribe(OnLoadRecordings);
        }

	    private void OnLoadRecordings()
	    {
            try
            {
                for (int i = 0; i < Recordings.Count; i++) 
                    Recordings.RemoveAt(i);

                foreach (var recording in TvHeadend.GetRecordings()) 
                    Recordings.Add(recording);

                RegionManager.Regions[RegionNames.RecordingRegion].RemoveAll();

                foreach (var recording in Recordings.OrderBy(r=>r.Start))
                {
                    var navigationParams = new NavigationParameters { { "DataContext", recording } };
                    RegionManager.RequestNavigate(RegionNames.RecordingRegion, new Uri("Recording", UriKind.Relative), NavigationCompleted, navigationParams);
                }

                EventAggregator.GetEvent<StatusChangedEvent>().Publish(new StatusInfo($"{Recordings.Count} Recordings."));
            }
            catch (Exception ex)
            {
                EventAggregator.GetEvent<StatusChangedEvent>().Publish(new StatusInfo(ex.Message));
            }
        }

        private void NavigationCompleted(NavigationResult obj)
        {
            //Console.WriteLine(obj.Context);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            OnLoadRecordings();
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
