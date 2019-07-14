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

        public string StartStop
        {
            get
            {
                var timespanUntilStart = StartReal.Subtract(DateTime.Now);
                var timespanUntilStop = StopReal.Subtract(DateTime.Now);

                var isNotRunningYet = timespanUntilStart.Milliseconds > 0;
                var isFinished = timespanUntilStop.Milliseconds < 0;

                var textPrefix = isNotRunningYet ? "Starting in " : isFinished ? "Finished since " : "Finished in ";
                var timeSpanUsed = isNotRunningYet ? timespanUntilStart : timespanUntilStop;

                var durationUntilStart = "";
                if (timeSpanUsed > TimeSpan.FromDays(1))
                    durationUntilStart = $" ({textPrefix}{Math.Abs(timeSpanUsed.Days)} days)";
                else if (timeSpanUsed > TimeSpan.FromHours(1))
                    durationUntilStart = $" ({textPrefix}{Math.Abs(timeSpanUsed.Hours)} hours)";
                else
                    durationUntilStart = $" ({textPrefix}{Math.Abs(timeSpanUsed.Minutes)} minutes)" ;

                if (Start.Date == Stop.Date)
                    return $"{Start:dd.MM. HH:mm} - {Stop:HH:mm}{durationUntilStart}";

                return $"{Start:dd.MM. HH:mm} - {Stop:dd.MM. HH:mm}{durationUntilStart}";
            }
        }

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
