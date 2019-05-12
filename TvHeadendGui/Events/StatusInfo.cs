using System.Windows;

namespace TvHeadendGui.Events
{
    public class StatusInfo
    {
        public StatusInfo(string statusText, int progressValue)
        {
            StatusText = statusText;
            ProgressValue = progressValue;
            ProgressBarVisibility = ProgressValue < 100 ? Visibility.Visible : Visibility.Hidden;
        }

        public StatusInfo(string statusText)
        {
            StatusText = statusText;
            ProgressBarVisibility = Visibility.Collapsed;
        }

        public string StatusText { get; set; }
        public int ProgressValue { get; set; }
        public Visibility ProgressBarVisibility { get; set; }
    }
}
