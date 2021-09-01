namespace DotVVM.Framework.Diagnostics.Models
{
    public class EventTiming
    {
        public EventTiming(string eventName, long duration, long totalDuration)
        {
            EventName = eventName;
            Duration = duration;
            TotalDuration = totalDuration;
        }

        public string EventName { get; set; }
        public long Duration { get; set; }
        public long TotalDuration { get; set; }
    }

}
