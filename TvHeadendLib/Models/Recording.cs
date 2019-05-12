using System;
using System.Security.Policy;

namespace TvHeadendLib.Models
{
    public class Recording : RecordingBase
    {
        public string ChannelName { get; set; }
        public DateTime StartReal { get; set; }
        public DateTime StopReal { get; set; }
        public string Status { get; set; }

        public string FileFullName { get; set; }
        public string Url { get; set; }

        public string StartStop => $"{Start:dd.MM. HH:mm} -> {Stop:dd.MM. HH:mm}";

        public override string ToString()
        {
            return $"{Start} -> {Stop}, {Title}, {ChannelName} (uuid: {Uuid})";
        }

        public string ToUserString()
        {
            return $"{Start:dd.MM. HH:mm} -> {Stop:dd.MM. HH:mm}, {Title}, {ChannelName}";
        }
    }
}
