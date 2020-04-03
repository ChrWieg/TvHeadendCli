namespace TvHeadendLib.Models
{
    public class Channel
    {
        public string ChannelKey { get; set; }
        public string ChannelName { get; set; }

        public int ChannelNumber { get; set; }

        //http://192.168.0.1:9981/play/stream/channelnumber/<channelnumber>

        ///stream/channel/47213311bfabd32256f010928f9ed9b3
        public string PlayLink => $"/stream/channel/{ChannelKey}";

        public override string ToString()
        {
            return $"{ChannelKey} : {ChannelName}";
        }
    }
}
