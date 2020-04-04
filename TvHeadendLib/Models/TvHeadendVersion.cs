using TvHeadendLib.Interfaces;

namespace TvHeadendLib.Models
{
    public class TvHeadendVersion : ITvHeadendObject
    {
        public string sw_version { get; set; }
        public string api_version { get; set; }
    }
}
