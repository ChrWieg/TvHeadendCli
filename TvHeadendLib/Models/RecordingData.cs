using System.Collections.Generic;

namespace TvHeadendLib.Models
{
    public class RecordingData
    {
        public List<RecordingEntry> Entries { get; set; }
        public int Total { get; set; }
    }
}
