using TvHeadendLib.Interfaces;

namespace TvHeadendLib.Models
{
    public class RecordingEntryBase : ITvHeadendObject
    {
        public string channel { get; set; }

        public LanguageData title { get; set; }
        public LanguageData subtitle { get; set; }
        public LanguageData description { get; set; }

        public long start { get; set; }
        public long stop { get; set; }

        public string comment { get; set; }

        public int pri { get; set; }

        public string url { get; set; }

        public string filename { get; set; }
    }
}
