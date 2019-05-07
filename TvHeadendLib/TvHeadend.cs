using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Extensions;
using TvHeadendLib.Helper;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;
using DataFormat = RestSharp.DataFormat;

namespace TvHeadendLib
{
    public class TvHeadend : ITvHeadend
    {
        //https://github.com/dave-p/TVH-API-docs/wiki

        private string _credentialPart = @"-un""{device_username}"" -up""{device_password}""";
        private string _createPart = @"-c""{channel_name_external}"" -t""{maxlength(title,""200"")}"" -p""{isset({production_year},'')}"" -d""{description}"" -r""TV-Browser"" -s{start_unix} -e{end_unix}";
        private string _removePart = @"-c""{channel_name_external}"" -s{start_unix}";
        private RestClient _restClient;
        private Uri _tvHeadendUri;
        private Credential _credentials;

        /// <summary>
        /// Instantiate and initialize TvHeadend.
        /// </summary>
        /// <param name="url">TvHeadend url and port: e.g. http://tvheadend:9981</param>
        /// <param name="useCredentialCache">True: credentials from Windows credential store will be used. False: no authentication.</param>
        public TvHeadend(string url, bool useCredentialCache)
        {
            TvHeadendBaseUri = new Uri(url);

            if (useCredentialCache)
                CredentialsFoundAtCredentialStore();

            InitializeRestClient();
        }

        /// <summary>
        /// Instantiate and initialize TvHeadend.
        /// Credentials from args will be used. If no credentials are provided, the application won't authenticate.
        /// Used by Command Line Tool TvHeadendCli
        /// </summary>
        /// <param name="args"></param>
        public TvHeadend(string[] args)
        {
            var url = GetUrlFromArgs(args);
            TvHeadendBaseUri = new Uri(url);
            Credentials = GetCredentialsFromArgs(args);
            InitializeRestClient();
        }

        /// <summary>
        /// Base URI of the TvHeadend server. Set this property to change base url of the rest client.
        /// </summary>
        public Uri TvHeadendBaseUri
        {
            get => _tvHeadendUri;
            set
            {
                _tvHeadendUri = value;
                if (_restClient != null)
                    _restClient.BaseUrl = _tvHeadendUri;
            }
        }

        /// <summary>
        /// Set the credentials for authentication if required. Set this property to change the authenticator of the RestClient.
        /// </summary>
        public Credential Credentials
        {
            get => _credentials;
            set
            {
                _credentials = value;
                if (_restClient != null && _credentials != null)
                    _restClient.Authenticator = new HttpBasicAuthenticator(_credentials.UserName, _credentials.Password);
            }
        }

        /// <summary>
        /// List of all enabled channels from the TvHeadend server.
        /// </summary>
        public ObservableCollection<Channel> Channels => GetChannels();

        /// <summary>
        /// List of all recordings from the TvHeadend server (scheduled, finished, failed...).
        /// </summary>
        public ObservableCollection<Recording> Recordings => GetRecordings();

        /// <summary>
        /// Creates a new recoding or schedule on the the TvHeadend server.
        /// </summary>
        /// <param name="recording">Start and Stop (unix time stamp), channel (uuid) and Title (file name) are required. Comment, Priority, SubTitle, Description are optional.</param>
        /// <returns>Recording with new recording uuid.</returns>
        public Recording CreateRecording(Recording recording)
        {
            //ToDo: implement post https://github.com/restsharp/RestSharp/wiki/ParameterTypes-for-RestRequest

            //Create
            //http://pihole:9981/api/dvr/entry/create?conf={"start":1555587900,"stop":1555596000,"channel":"f1351106ed1b6872d85bbf2eab0e93c9","pri":2,"title":{"ger":"Die Prinzessin von Montpensier"},"subtitle":{"ger":"(2009)"}}

            //if the recording exists return existing
            if (TryGetRecordingUuidFromChannelAndStart(recording))
                return recording;

            //create new recording entry
            var recordingEntry = new RecordingEntryBase
            {
                channel = recording.Channel,
                comment = recording.Comment.UrlEncode(),
                pri = recording.Priority,
                start = UnixTimeConverter.GetUnixTimeFromDateTime(recording.Start.ToUniversalTime()),
                stop =  UnixTimeConverter.GetUnixTimeFromDateTime(recording.Stop.ToUniversalTime()),
                title = new LanguageData { ger = recording.Title.UrlEncode() },
                subtitle = new LanguageData { ger = recording.SubTitle.UrlEncode() },
                description = new LanguageData { ger = recording.Description.UrlEncode() }
            };

            //serialize new recording to json
            var jsonString = GetJsonStringFromObject(recordingEntry);

            if (string.IsNullOrWhiteSpace(jsonString))
                throw new InvalidOperationException("Unable to serialize recording object to json");

            //create tvheadend-command
            var command = "/api/dvr/entry/create?conf=" + jsonString;

            var request = new RestRequest(command, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                AlwaysMultipartFormData = true
            };

            var response = _restClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new UnauthorizedAccessException($"The Server has denied the operation: {response.StatusCode}");

            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
                throw new HttpRequestException("Web-request failed!");

            var recordingUuid = GetObjectFromJsonSting<RecordingUuid>(response.Content);

            recording.Uuid = recordingUuid.uuid;

            return recording;
        }

        /// <summary>
        /// Remove the given recording.
        /// </summary>
        /// <param name="recording">Recording Uuid required.</param>
        /// <returns>True: successful, False: failed.</returns>
        public bool RemoveRecordingSchedule(Recording recording)
        {
            if (!TryGetRecordingUuidFromChannelAndStart(recording))
                throw new Exception("Unable to get Recording-Uuid from channel and start time.", null);

            //Remove scheduled recording
            //http://pihole:9981/api/dvr/entry/cancel?uuid=c6c86a720748ef3ea880c706a22cd776
            var removeRecordingCommand = "/api/dvr/entry/cancel?uuid=" + recording.Uuid;

            var request = new RestRequest(removeRecordingCommand, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                AlwaysMultipartFormData = true
            };

            var response = _restClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new UnauthorizedAccessException($"The Server has denied the operation: {response.StatusCode}");

            return response.IsSuccessful;
        }

        /// <summary>
        /// Deletes a finished recording and the file from the Tvheadend server.
        /// </summary>
        /// <param name="recording">Recording Uuid required.</param>
        /// <returns>True: successful, False: failed.</returns>
        public bool DeleteRecordedFile(Recording recording)
        {
            if (!TryGetRecordingUuidFromChannelAndStart(recording))
                throw new Exception("Unable to get Recording-Uuid from channel and start time.", null);

            //Remove scheduled recording
            //http://pihole:9981/api/dvr/entry/cancel?uuid=c6c86a720748ef3ea880c706a22cd776
            var removeRecordingCommand = "/api/dvr/entry/remove?uuid=" + recording.Uuid;

            var request = new RestRequest(removeRecordingCommand, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                AlwaysMultipartFormData = true
            };

            var response = _restClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new UnauthorizedAccessException($"The Server has denied the operation: {response.StatusCode}");

            return response.IsSuccessful;
        }

        /// <summary>
        /// Try to parse and deserialise recording from command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments providing the recording data.</param>
        /// <returns>Instance of a Recording</returns>
        public Recording GetRecordingFromArgs(string[] args) //, int firstIndex
        {
            var result = new Recording {Priority = 2};

            for (var i = 1; i < args.Length; i++) //first arg is application Path
            {
                switch (args[i].Substring(0,2).ToLower())
                {
                    case "-c":
                        result.ChannelName = args[i].Substring(2).Trim();
                        break;
                    case "-t":
                        result.Title = args[i].Substring(2).Trim();
                        break;
                    case "-p": //production year
                        result.SubTitle = args[i].Substring(2).Trim();
                        break;
                    case "-d":
                        result.Description = args[i].Substring(2).Trim();
                        break;
                    case "-r":
                        result.Comment = args[i].Substring(2).Trim();
                        break;
                    case "-s":
                        if (long.TryParse(args[i].Substring(2).Trim(), out var startUnixTimestamp))
                            result.Start = UnixTimeConverter.GetDateTimeFromUnixTime(startUnixTimestamp).ToLocalTime();
                        break;
                    case "-e":
                        if (long.TryParse(args[i].Substring(2).Trim(), out var stopUnixTimestamp))
                            result.Stop = UnixTimeConverter.GetDateTimeFromUnixTime(stopUnixTimestamp).ToLocalTime();
                        break;
                }
            }

            return TryGetChannelUuidFromName(result) ? result : null;
        }

        /// <summary>
        /// Tests configuration and availability of the rest client.
        /// </summary>
        /// <returns>Rest status code or rest error exception or exception message</returns>
        public string RestClientIsWorking()
        {
            if (_restClient == null)
                return "Rest client not initialized yet";

            try
            {
                var command = "/api/channel/list";
                var request = new RestRequest(command, Method.GET)
                {
                    RequestFormat = DataFormat.Json,
                    AlwaysMultipartFormData = true
                };

                var response = _restClient.Execute(request);
                if (!response.IsSuccessful)
                {
                    return $"Rest Client Error: {response.StatusCode.ToString()}" ;
                }

                if (response.ErrorException != null)
                    return $"Rest Client Error: {response.ErrorException.Message}" ;
            }
            catch (WebException ex)
            {
                return ex.Message;
            }

            return "Rest Client: Okay";
        }

        /// <summary>
        /// First configure TvHeadend and test the rest client (RestClientIsWorking).
        /// Call this method to get the create-parameter string for Aufnahmesteuerung.
        /// </summary>
        /// <returns>Create-Parameters for Aufnahmesteuerung</returns>
        public string GetCreateParameterString()
        {
            var result = @"-acreate";
            result += $" -url\"{TvHeadendBaseUri.AbsoluteUri}\"";
            if (Credentials != null)
                result += " " + _credentialPart;
            result += " " + _createPart;

            return result;
        }

        /// <summary>
        /// First configure TvHeadend and test the rest client (RestClientIsWorking).
        /// Call this method to get the remove-parameter string for Aufnahmesteuerung.
        /// </summary>
        /// <returns>Remove-Parameters for Aufnahmesteuerung</returns>
        public string GetRemoveParameterString()
        {
            var result = @"-aremove";
            result += $" -url\"{TvHeadendBaseUri.AbsoluteUri}\"";
            if (Credentials != null)
                result += " " + _credentialPart;
            result += " " + _removePart;

            return result;
        }

        private void InitializeRestClient()
        {
            _restClient = new RestClient(TvHeadendBaseUri.AbsoluteUri);
            if (Credentials == null)
                return;

            var httpBasicAuthenticator = new HttpBasicAuthenticator(Credentials.UserName, Credentials.Password);
            _restClient.Authenticator = httpBasicAuthenticator;
        }

        private void CredentialsFoundAtCredentialStore()
        {
            Credentials = CredentialHelper.GetStoredCredential(false);
        }

        private ObservableCollection<Channel> GetChannels()
        {
            var command = "/api/channel/list";
            var request = new RestRequest(command, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                AlwaysMultipartFormData = true
            };

            var response = _restClient.Execute<ChannelData>(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new UnauthorizedAccessException($"The Server has denied the operation: {response.StatusCode}");

            var list = response?.Data?.Entries.Select(e => new Channel {ChannelName = e.Val, ChannelKey = e.Key}).OrderBy(e=>e.ChannelName).ToList();

            return list != null ? new ObservableCollection<Channel>(list) : new ObservableCollection<Channel>();
        }

        private ObservableCollection<Recording> GetRecordings()
        {
            //List
            //http://pihole:9981/api/dvr/entry/grid

            var getRecordingsCommand = "/api/dvr/entry/grid"; //grid_upcoming, grid_finished, grid_failed, grid_removed, 
            var request = new RestRequest(getRecordingsCommand, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                AlwaysMultipartFormData = true
            };

            var response = _restClient.Execute<RecordingData>(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new UnauthorizedAccessException($"The Server has denied the operation: {response.StatusCode}");

            var result = response.Data?.Entries?.Select(e =>
                new Recording
                {
                    Uuid = e.Uuid,
                    Channel = e.channel,
                    ChannelName = e.Channelname,
                    Title = e.title.ger,
                    Comment = e.comment,
                    Priority = e.pri,
                    Start = UnixTimeConverter.GetDateTimeFromUnixTime(e.start).ToLocalTime(),
                    Stop = UnixTimeConverter.GetDateTimeFromUnixTime(e.stop).ToLocalTime(),
                    StartReal = UnixTimeConverter.GetDateTimeFromUnixTime(e.Start_real).ToLocalTime(),
                    StopReal = UnixTimeConverter.GetDateTimeFromUnixTime(e.Stop_real).ToLocalTime(),
                    Status = e.Status,
                    FileFullName = e.filename,
                    Url = e.url
                }).ToList();

            return result != null ? new ObservableCollection<Recording>(result.OrderBy(r=>r.Start)) : new ObservableCollection<Recording>();
        }

        private bool TryGetChannelUuidFromName(Recording recording)
        {
            var result = GetChannels().Where(c => NormalizeChannelName(c.ChannelName) == NormalizeChannelName(recording.ChannelName)).Select(c2 => c2.ChannelKey).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(result))
                return false;

            recording.Channel = result;

            return true;
        }

        private bool TryGetRecordingUuidFromChannelAndStart(Recording recording)
        {
            var result = GetRecordings().FirstOrDefault(c => c.Channel == recording.Channel && c.Start == recording.Start);
            if (string.IsNullOrWhiteSpace(result?.Uuid))
                return false;

            recording.Uuid = result.Uuid;

            return true;
        }

        private string NormalizeChannelName(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                return null;

            var result = channelName;
            result = result.Replace(" ", "").Replace("_","").Replace("-", "").Replace(".", "").ToLower();
            return result;
        }

        private static string GetJsonStringFromObject<T>(T objectToConvert) where T: ITvHeadendObject
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));

            using (var memoryStream = new MemoryStream())
            {
                jsonSerializer.WriteObject(memoryStream, objectToConvert);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var jsonString = streamReader.ReadToEnd();
                    streamReader.Close();
                    memoryStream.Close();
                    return jsonString;
                }
            }
        }

        private static T GetObjectFromJsonSting<T>(string jsonString) where T: ITvHeadendObject, new()
        {
            var jsonDeSerializer = new DataContractJsonSerializer(typeof(T));

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                var result = (T)jsonDeSerializer.ReadObject(memoryStream);
                return result;
            }
        }

        private Credential GetCredentialsFromArgs(string[] args)
        {
            var userName = "";
            var password = "";

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i].Substring(0, 3).ToLower())
                {
                    case "-un":
                        userName = args[i].Substring(3).Trim();
                        break;
                    case "-up":
                        password = args[i].Substring(3).Trim();
                        break;
                }

            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
                return new Credential {Password = password, UserName = userName};
            }

            return null;
        }

        private string GetUrlFromArgs(string[] args)
        {
            foreach (var t in args)
            {
                if (t.Substring(0, 4).ToLower() == "-url")
                    return t.Substring(4).Trim();
            }

            return null;
        }
    }
}
