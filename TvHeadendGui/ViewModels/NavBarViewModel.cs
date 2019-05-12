using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using TvHeadendGui.Helper;
using TvHeadendGui.Views;
using TvHeadendLib;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui.ViewModels
{
	public class NavBarViewModel : ViewModelBase
    {
        public DelegateCommand ControlLoaded { get; set; }

        public DelegateCommand NavigateToChannels { get; set; }
        public DelegateCommand NavigateToRecordings { get; set; }
        public DelegateCommand NavigateToSettings { get; set; }
        public bool IsOpen { get; set; }

        public NavBarViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ITvHeadend tvHeadend) : base(regionManager, eventAggregator, tvHeadend)
        {
            ControlLoaded = new DelegateCommand(OnControlLoaded);
            NavigateToChannels = new DelegateCommand(OnNavigateToChannels);
            NavigateToRecordings = new DelegateCommand(OnNavigateToRecordings);
            NavigateToSettings = new DelegateCommand(OnNavigateToSettings);
        }

        private void OnControlLoaded()
        {
            OnNavigateToRecordings();
        }

        private void OnNavigateToRecordings()
        {
            RegionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Recordings));
        }

        private void OnNavigateToChannels()
	    {
	        RegionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Channels));
        }

        private void OnNavigateToSettings()
        {
            RegionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Settings));
        }
    }
}
