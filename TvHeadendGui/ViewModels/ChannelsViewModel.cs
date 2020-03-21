using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using TvHeadendGui.Events;
using TvHeadendGui.Properties;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadendGui.ViewModels
{
	public class ChannelsViewModel : ViewModelBase, INavigationAware
    {
	    public ObservableCollection<Channel> Channels { get; set; }

	    public DelegateCommand ExportChannels { get; set; }
	    public DelegateCommand ReloadChannels { get; set; }

        public DelegateCommand MouseDoubleClickCommand { get; set; }

        public Channel SelectedChannel { get; set; }

	    public ChannelsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ITvHeadend tvHeadend) : base(regionManager, eventAggregator, tvHeadend)
	    {
            ReloadChannels = new DelegateCommand(OnReloadChannels);
	        ExportChannels = new DelegateCommand(OnExportChannels);
            MouseDoubleClickCommand = new DelegateCommand(OnMouseDoubleClick);
	    }

        private void OnMouseDoubleClick()
        {
            //ToDo C: path to player and pattern as variable - Problem: authentication
            var playerPath = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";
            var uri = $"{TvHeadend.TvHeadendBaseUri}{SelectedChannel.PlayLink}";

            var vlc = new Process {StartInfo = {FileName = playerPath, Arguments = uri}};
            vlc.Start();
        }

        private void OnReloadChannels()
	    {
	        try
	        {
                Channels?.Clear();
                Channels = TvHeadend.GetChannels();
                EventAggregator.GetEvent<StatusChangedEvent>().Publish(new StatusInfo($"{Channels.Count} Channels."));
	        }
	        catch (System.Exception ex)
	        {
	            EventAggregator.GetEvent<StatusChangedEvent>().Publish(new StatusInfo(ex.Message));
	        }
        }

	    private void OnExportChannels()
	    {
	        var executablePath = Path.GetDirectoryName(Application.ExecutablePath);

	        if (executablePath == null) return;

            //ToDo B: file picker
	        var fileFullName = Path.Combine(executablePath, "Channels.txt");
	        //var fileDialog = new OpenFileDialog {Filter = "(*.txt)|*.txt", FileName = "Channels.txt"};
	        //var result = fileDialog.ShowDialog();
	        //if (result != DialogResult.OK || !string.IsNullOrWhiteSpace(fileDialog.FileName)) return;

	        var channelList = Channels.Select(c => c.ChannelName).OrderBy(c=>c).ToArray();
	        File.WriteAllLines(fileFullName, channelList);

            if (File.Exists(fileFullName))
                Process.Start(fileFullName);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            OnReloadChannels();
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
