using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Prism.Commands;
using Prism.Regions;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadendGui.ViewModels
{
	public class ChannelsViewModel : ViewModelBase
	{
	    public ObservableCollection<Channel> Channels { get; set; }

	    public DelegateCommand ExportChannels { get; set; }


        public ChannelsViewModel(IRegionManager regionManager, ITvHeadend tvHeadend) : base(regionManager, tvHeadend)
        {
            Channels = TvHeadend.Channels;
            ExportChannels = new  DelegateCommand(OnExportChannels);
        }

	    private void OnExportChannels()
	    {
	        var executablePath = Path.GetDirectoryName(Application.ExecutablePath);

	        if (executablePath == null) return;

            //ToDo: file picker
	        var fileFullName = Path.Combine(executablePath, "Channels.txt");
	        //var fileDialog = new OpenFileDialog {Filter = "(*.txt)|*.txt", FileName = "Channels.txt"};
	        //var result = fileDialog.ShowDialog();

	        //if (result != DialogResult.OK || !string.IsNullOrWhiteSpace(fileDialog.FileName)) return;

	        var channelList = Channels.Select(c => c.ChannelName).OrderBy(c=>c).ToArray();
	        File.WriteAllLines(fileFullName, channelList);
	    }
	}
}
