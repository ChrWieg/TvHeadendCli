using System;
using Microsoft.Win32;

namespace TvHeadendGui.Helper
{

    public class GetDotNetVersion
    {
        //https://docs.microsoft.com/de-de/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed?view=netframework-4.8

        public static string Get45PlusFromRegistry()
        {
            const string subKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subKey))
            {
                var key = ndpKey?.GetValue("Release").ToString();
                if (key != null && int.TryParse(key, out var version))
                    return $"Installed .NET Framework Version: {CheckFor45PlusVersion(version)}.";

                return ".NET Framework Version 4.5 or later is not installed.";
            }
        }

        private static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 528040) return "4.8 or later";
            if (releaseKey >= 461808) return "4.7.2";
            if (releaseKey >= 461308) return "4.7.1";
            if (releaseKey >= 460798) return "4.7";
            if (releaseKey >= 394802) return "4.6.2";
            if (releaseKey >= 394254) return "4.6.1";
            if (releaseKey >= 393295) return "4.6";
            if (releaseKey >= 379893) return "4.5.2";
            if (releaseKey >= 378675) return "4.5.1";
            if (releaseKey >= 378389) return "4.5";

            return "No 4.5 or later version detected";
        }
    }
}
