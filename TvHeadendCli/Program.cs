using System;
using TvHeadendLib;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadendCli
{
    static class Program
    {

        private static readonly string CreateParametersExample = @"-acreate -url""http://pihole:9981"" -un""{device_username}"" -up""{device_password}"" -c""{channel_name_external}"" -t""{maxlength(title,""200"")}"" -p""{production_year}"" -d""{description}"" -r""TV-Browser"" -s{start_unix} -e{end_unix}";
        private static readonly string RemoveParametersExample = @"-aremove -url""http://pihole:9981"" -un""{device_username}"" -up""{device_password}"" -c""{channel_name_external}"" -s{start_unix}";
        
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            //ToDo: Test Error Handling

            try
            {
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

                //Try to get recording info from args
                var recording = tvHeadend.GetRecordingFromArgs(args, 1);
                if (recording == null)
                {
                    Console.WriteLine("Error: unable to resolve channel name");
                    return -1;
                }

                //Action
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

        private static int RemoveRecording(ITvHeadend tvHeadend, Recording recording)
        {
            if (!tvHeadend.RemoveRecording(recording))
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

        private static void WriteHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Command Line Examples:");
            Console.WriteLine();
            Console.WriteLine("Tv-Headend Create Recording:");
            Console.WriteLine(CreateParametersExample);
            Console.WriteLine();
            Console.WriteLine("Tv-Headend Remove Recording:");
            Console.WriteLine(RemoveParametersExample);
            Console.WriteLine();
            Console.WriteLine("Press Enter to quit");
            Console.ReadLine();
        }
    }
}
