namespace TvHeadendLib.Models
{
    public class RecordingEntry : RecordingEntryBase
    {
        public string Uuid { get; set; }

        public string Channelname { get; set; }

        public long Start_real { get; set; }
        public long Stop_real { get; set; }

        public string Status { get; set; }

    }
}
