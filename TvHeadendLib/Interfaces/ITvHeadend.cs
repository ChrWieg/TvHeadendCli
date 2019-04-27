using System.Collections.ObjectModel;
using TvHeadendLib.Models;

namespace TvHeadendLib.Interfaces
{
    public interface ITvHeadend
    {
        ObservableCollection<Channel> Channels { get; }
        ObservableCollection<Recording> Recordings { get; }

        Recording CreateRecording(Recording recording);
        bool RemoveRecording(Recording recording);

        Recording GetRecordingFromArgs(string[] args, int firstIndex);
    }
}