using System;

namespace TvHeadendLib.Models
{
    public class RecordingBase
    {
        public string Uuid { get; set; }
        public string Channel { get; set; }

        public string Title { get; set; }
        public string SubTitle { get; set; } = "";
        public string Description { get; set; } = "";

        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }

        public string Comment { get; set; } = "";

        public int Priority { get; set; }

        public override string ToString()
        {
            return $"{Start} -> {Stop}, {Title}, {SubTitle}, {Channel}";
        }

    }
}
