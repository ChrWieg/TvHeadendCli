using System;

namespace TvHeadendLib.Models
{
    public class Channel
    {
        public string ChannelKey { get; set; }
        public string ChannelName { get; set; }

        public override string ToString()
        {
            return $"{ChannelKey} : {ChannelName}";
        }
    }
}
