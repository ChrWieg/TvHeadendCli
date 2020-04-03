using System.Reflection;
using System.Windows;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using TvHeadendGui.Events;
using TvHeadendGui.Helper;
using TvHeadendGui.Views;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public DelegateCommand ContentRendered { get; set; }
        public string Title { get; set; }

        public string StatusText { get; set; }
        public int ProgressValue { get; set; }
        public Visibility ProgressBarVisibility { get; set; }

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ITvHeadend tvHeadend) : base(regionManager, eventAggregator, tvHeadend)
        {
            ProgressBarVisibility = Visibility.Collapsed;
            Title = "TV-Headend GUI " + Assembly.GetExecutingAssembly().GetName().Version;
            StatusText = "TvHeadEndGui loaded.";
            EventAggregator.GetEvent<StatusChangedEvent>().Subscribe(OnStatusChanged);
            ContentRendered = new DelegateCommand(OnContentRendered);
        }

        private void OnStatusChanged(StatusInfo statusInfo)
        {
            StatusText = statusInfo.StatusText;
        }

        private void OnContentRendered()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavBarRegion, typeof(NavBar));
            RegionManager.RegisterViewWithRegion(RegionNames.RecordingRegion, typeof(Recording));
            RegionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(Recordings));
            RegionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(Channels));
            RegionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(Settings));
        }
    }
}
