using TvHeadendLib.Interfaces;

namespace TvHeadendLib.Models
{
    public class RecordingUuid : ITvHeadendObject
    {
        public string uuid { get; set; }

        public override string ToString()
        {
            return uuid;
        }
    }
}
