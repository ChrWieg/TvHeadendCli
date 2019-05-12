using System.Windows;
using System.Windows.Controls;
using Prism.Regions;

namespace TvHeadendGui.Helper
{
    //ToDo: implement and test
    public class StackPanelRegionAdapter : RegionAdapterBase<StackPanel>
    {
        public StackPanelRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, StackPanel regionTarget)
        {
            //ToDo: Test cases
            region.Views.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (FrameworkElement element in e.NewItems)
                            regionTarget.Children.Add(element);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (FrameworkElement element in e.OldItems)
                            regionTarget.Children.Remove(element);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        foreach (FrameworkElement element in e.OldItems)
                            regionTarget.Children.Remove(element);
                        foreach (FrameworkElement element in e.NewItems)
                            regionTarget.Children.Add(element);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        break;
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
