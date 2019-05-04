using Prism.Mvvm;
using Prism.Regions;
using TvHeadendLib;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        public IRegionManager RegionManager { get; set; }

        public ITvHeadend TvHeadend { get; set; }

        public ViewModelBase(IRegionManager regionManager, ITvHeadend tvHeadend)
        {
            RegionManager = regionManager;
            TvHeadend = tvHeadend;
        }
    }
}
