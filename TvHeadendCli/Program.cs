using System;
using System.Text.RegularExpressions;
using TvHeadendLib;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadendCli
{
    static class Program
    {

        private static readonly string CreateParametersExample = @"-acreate -url""http://tvheadend:9981"" -un""{device_username}"" -up""{device_password}"" -c""{channel_name_external}"" -t""{maxlength(title,""200"")}"" -p""{isset({production_year},'')}"" -d""{description}"" -r""TV-Browser"" -s{start_unix} -e{end_unix}";
        private static readonly string RemoveParametersExample = @"-aremove -url""http://tvheadend:9981"" -un""{device_username}"" -up""{device_password}"" -c""{channel_name_external}"" -s{start_unix}";
        private static readonly string GetChannelNamesExample = @"-channels -url""http://tvheadend:9981"" -un""device_username"" -up""device_password""";

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            //ToDo: Test Error Handling

            try
            {
                args = PrepareArgs(args);

                //first parameter is -help: reply help info then return
                if (args[0].ToLower().StartsWith("-help"))
                {
                    WriteHelp();
                    return 0;
                }

                //Check if at least more than 2 parameters are given, if not reply help info then return
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: insufficient arguments!");
                    WriteHelp();
                    return -1;
                }

                ITvHeadend tvHeadend = new TvHeadend(args);

                //Action -channels
                if (args[0].ToLower().StartsWith("-channels"))
                    return GetChannels(tvHeadend);

                //Try to get recording info from args
                var recording = tvHeadend.GetRecordingFromArgs(args); // , 1
                if (recording == null)
                {
                    Console.WriteLine("Error: unable to resolve channel name");
                    return -1;
                }

                //Recording Actions
                if (args[0].ToLower().StartsWith("-acreate"))
                    return CreateRecording(tvHeadend, recording);

                if (args[0].ToLower().StartsWith("-aremove"))
                    return RemoveRecording(tvHeadend, recording);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }

            return 0;
        }

        private static string[] PrepareArgs(string[] args)
        {
            var joinedArgs = string.Join(" ", args);

            var regEx = new Regex(@"\s(-[a-zA-Z]{1,2})");
            var tempArgs = regEx.Replace(joinedArgs, "#####$1");

            regEx = new Regex(@"#####");
            var resultArgs = regEx.Split(tempArgs);

            return resultArgs;
        }

        private static int RemoveRecording(ITvHeadend tvHeadend, Recording recording)
        {
            if (!tvHeadend.RemoveRecordingSchedule(recording))
            {
                Console.WriteLine("Error: removing failed!");
                return -1;
            }

            Console.WriteLine($"Success: Recording removed {recording.ToUserString()}.");
            return 0;
        }

        private static int CreateRecording(ITvHeadend tvHeadend, Recording recording)
        {
            var result = tvHeadend.CreateRecording(recording);
            if (!string.IsNullOrWhiteSpace(result?.Uuid))
            {
                Console.WriteLine($"Success: Recording created: {recording.ToUserString()}");
                return 0;
            }

            Console.WriteLine($"Error: Recording not created: {recording.ToUserString()}");
            return -1;
        }

        private static int GetChannels(ITvHeadend tvHeadend)
        {
            Console.WriteLine("Channel names from TvHeadend:");

            foreach (var tvHeadendChannel in tvHeadend.GetChannels())
                Console.WriteLine(tvHeadendChannel.ChannelName);
            return 0;
        }

        private static void WriteHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Command Line Examples:");
            Console.WriteLine();
            Console.WriteLine("Tv-Headend Create Recording with TV-Browser Capture-Plugin:");
            Console.WriteLine(CreateParametersExample);
            Console.WriteLine();
            Console.WriteLine("Tv-Headend Remove Recording with TV-Browser Capture-Plugin:");
            Console.WriteLine(RemoveParametersExample);
            Console.WriteLine();
            Console.WriteLine("Get channel names from Tv-Headend:");
            Console.WriteLine(GetChannelNamesExample);
            Console.WriteLine();
            Console.WriteLine("Use pipe: > Channels.txt to redirekt output into a file.");
        }
    }
}
