using System;
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
            try
            {
                var url = TryGetUrlFromSettings();
                var useCredentialCache = TvHeadendGui.Properties.Settings.Default.AuthenticationRequired && TvHeadendGui.Properties.Settings.Default.SaveCredentialsToWindowsCredentialStore;

                containerRegistry.RegisterInstance(typeof(IRegionManager), new RegionManager());

                containerRegistry.RegisterSingleton(typeof(ITvHeadend));
                containerRegistry.RegisterInstance(typeof(ITvHeadend), new TvHeadend(url, useCredentialCache));

                containerRegistry.Register(typeof(object), typeof(NavBar), nameof(NavBar));
                containerRegistry.Register(typeof(object), typeof(Channels), nameof(Channels));
                containerRegistry.Register(typeof(object), typeof(Recordings), nameof(Recordings));
                containerRegistry.Register(typeof(object), typeof(Recording), nameof(Recording));
                containerRegistry.Register(typeof(object), typeof(Settings), nameof(Settings));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string TryGetUrlFromSettings()
        {
            if (TvHeadendGui.Properties.Settings.Default == null)
                return "http://TvHeadend:9981";

            var uriBuilder = new UriBuilder
            {
                Host = TvHeadendGui.Properties.Settings.Default.ServerName,
                Path = TvHeadendGui.Properties.Settings.Default.ServerPath,
                Port = TvHeadendGui.Properties.Settings.Default.PortNumber,
                Scheme = TvHeadendGui.Properties.Settings.Default.UseTls ? "https" : "http"
            };

            return uriBuilder.Uri.ToString();
            //var serverName = TvHeadendGui.Properties.Settings.Default.ServerName;
            //var serverPath = TvHeadendGui.Properties.Settings.Default.ServerPath;
            //var portNumber = TvHeadendGui.Properties.Settings.Default.PortNumber;

            //var protocol = TvHeadendGui.Properties.Settings.Default.UseTls ? "https://" : "http://";
            //return $"{protocol}{serverName}:{portNumber}{serverPath}";
        }
    }
}
