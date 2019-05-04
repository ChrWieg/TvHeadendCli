using Prism.Commands;
using Prism.Regions;
using TvHeadendGui.Views;
using TvHeadendLib;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui.ViewModels
{
	public class NavBarViewModel : ViewModelBase
    {
	    public DelegateCommand NavigateToChannels { get; set; }
        public DelegateCommand NavigateToRecordings { get; set; }
        public DelegateCommand NavigateToSettings { get; set; }
        public bool IsOpen { get; set; }

        public NavBarViewModel(IRegionManager regionManager, ITvHeadend tvHeadend) : base(regionManager, tvHeadend)
        {
            NavigateToChannels = new DelegateCommand(OnNavigateToChannels);
            NavigateToRecordings = new DelegateCommand(OnNavigateToRecordings);
            NavigateToSettings = new DelegateCommand(OnNavigateToSettings);
        }

        private void OnNavigateToSettings()
        {
            RegionManager.RequestNavigate("ContentRegion", nameof(Settings));
        }

        private void OnNavigateToRecordings()
        {
            RegionManager.RequestNavigate("ContentRegion", nameof(Recordings));
        }

        private void OnNavigateToChannels()
	    {
	        RegionManager.RequestNavigate("ContentRegion", nameof(Channels));
        }
    }
}
