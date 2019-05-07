using System;
using System.Collections.ObjectModel;
using TvHeadendLib.Models;

namespace TvHeadendLib.Interfaces
{
    public interface ITvHeadend
    {
        /// <summary>
        /// Base URI of the TvHeadend server. Set this property to change the base url of the RestClient.
        /// </summary>
        Uri TvHeadendBaseUri { get; set; }

        /// <summary>
        /// Set the credentials for authentication if required. Set this property to change the authenticator of the RestClient.
        /// </summary>
        Credential Credentials { get; set; }

        /// <summary>
        /// List of all enabled channels from the TvHeadend server.
        /// </summary>
        ObservableCollection<Channel> Channels { get; }

        /// <summary>
        /// List of all recordings from the TvHeadend server (scheduled, finished, failed...).
        /// </summary>
        ObservableCollection<Recording> Recordings { get; }

        /// <summary>
        /// Creates a new recoding or schedule on the the TvHeadend server.
        /// </summary>
        /// <param name="recording">Start and Stop (unix time stamp), channel (uuid) and Title (file name) are required. Comment, Priority, SubTitle, Description are optional.</param>
        /// <returns>Recording with new recording uuid.</returns>
        Recording CreateRecording(Recording recording);

        /// <summary>
        /// Remove the given recording.
        /// </summary>
        /// <param name="recording">Recording Uuid required.</param>
        /// <returns>True: successful, False: failed.</returns>
        bool RemoveRecordingSchedule(Recording recording);

        /// <summary>
        /// Deletes a finished recording and the file from the Tvheadend server.
        /// </summary>
        /// <param name="recording">Recording Uuid required.</param>
        /// <returns>True: successful, False: failed.</returns>
        bool DeleteRecordedFile(Recording recording);

        /// <summary>
        /// Try to parse and deserialise recording from command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments providing the recording data.</param>
        /// <returns>Instance of a Recording</returns>
        Recording GetRecordingFromArgs(string[] args);

        /// <summary>
        /// Tests configuration and availability of the rest client.
        /// </summary>
        /// <returns>Rest status code or rest error exception or exception message</returns>
        string RestClientIsWorking();

        /// <summary>
        /// First configure TvHeadend and test the rest client (RestClientIsWorking).
        /// Call this method to get the create-parameter string for Aufnahmesteuerung.
        /// </summary>
        /// <returns>Create-Parameters for Aufnahmesteuerung</returns>
        string GetCreateParameterString();

        /// <summary>
        /// First configure TvHeadend and test the rest client (RestClientIsWorking).
        /// Call this method to get the remove-parameter string for Aufnahmesteuerung.
        /// </summary>
        /// <returns>Remove-Parameters for Aufnahmesteuerung</returns>
        string GetRemoveParameterString();

    }
}