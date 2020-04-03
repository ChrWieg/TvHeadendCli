using System;

namespace TvHeadendLib.Helper
{
    public class WinHelper
    {
        public static string ValidateFileName(string originalFilename)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            var newFileName = originalFilename.Replace("\n", " ").Replace("\r", " ");
            newFileName = string.Join(" ", newFileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).Trim();
            newFileName = newFileName.Replace("  ", " ");
            return newFileName;
        }
    }
}
