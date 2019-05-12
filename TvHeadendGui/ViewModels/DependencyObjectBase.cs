using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Events;
using Prism.Regions;
using TvHeadendGui.Annotations;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui.ViewModels
{
    public class DependencyObjectBase: DependencyObject, INotifyPropertyChanged
    {
        public IRegionManager RegionManager { get; set; }
        public IEventAggregator EventAggregator { get; set; }

        public ITvHeadend TvHeadend { get; set; }

        public DependencyObjectBase(IRegionManager regionManager, IEventAggregator eventAggregator, ITvHeadend tvHeadend)
        {
            RegionManager = regionManager;
            EventAggregator = eventAggregator;
            TvHeadend = tvHeadend;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
