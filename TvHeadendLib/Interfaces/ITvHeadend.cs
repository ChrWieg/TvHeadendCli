using System;
using System.Collections.ObjectModel;
using System.Net;
using TvHeadendLib.Models;

namespace TvHeadendLib.Interfaces
{
    public interface ITvHeadend
    {
        Uri TvHeadendUri { get; set; }
        ObservableCollection<Channel> Channels { get; }
        ObservableCollection<Recording> Recordings { get; }

        Recording CreateRecording(Recording recording);
        bool RemoveRecordingSchedule(Recording recording);
        bool DeleteRecordedFile(Recording recording);

        Recording GetRecordingFromArgs(string[] args); //, int firstIndex
        string RestClientIsWorking();

        Credential Credentials { get; set; }

        string GetCreateParameterString();
        string GetRemoveParameterString();

    }
}