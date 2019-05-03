using System.Windows;
using Prism.Ioc;
using Prism.Regions;
using TvHeadendGui.Views;
using TvHeadendLib;
using TvHeadendLib.Interfaces;

namespace TvHeadendGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var url = TryGetUrlFromSettings();

            containerRegistry.RegisterInstance(typeof(IRegionManager), new RegionManager());
            containerRegistry.RegisterInstance(typeof(ITvHeadend), new TvHeadend(url,true));

            containerRegistry.Register(typeof(object), typeof(NavBar), nameof(NavBar));
            containerRegistry.Register(typeof(object), typeof(Channels), nameof(Channels));
            containerRegistry.Register(typeof(object), typeof(Recordings), nameof(Recordings));
        }

        private string TryGetUrlFromSettings()
        {
            //ToDo: Settings / Parameter
            return "http://pihole:9981";
        }
    }
}
