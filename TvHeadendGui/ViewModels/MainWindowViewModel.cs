using Prism.Commands;
using Prism.Regions;
using TvHeadendGui.Views;
using TvHeadendLib;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public DelegateCommand ContentRendered { get; set; }
        public string Title { get; set; }

        public MainWindowViewModel(IRegionManager regionManager, ITvHeadend tvHeadend) : base(regionManager, tvHeadend)
        {
            Title = "TV-Headend Client";
            ContentRendered = new DelegateCommand(OnContentRendered);
        }

        private void OnContentRendered()
        {
            RegionManager.RegisterViewWithRegion("NavBarRegion", typeof(NavBar));
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(Recordings));
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(Channels));
        }
    }
}
