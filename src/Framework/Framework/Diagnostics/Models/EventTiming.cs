namespace DotVVM.Framework.Diagnostics.Models
{
    public record EventTiming(string EventName, long Duration, long TotalDuration, bool StartupComplete = false)
    {
    }
}
