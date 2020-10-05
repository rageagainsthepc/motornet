using System.Diagnostics;
using CloudNative.CloudEvents.Extensions;

namespace Motor.Extensions.Diagnostics.Tracing
{
    public static class DistributedTracingExtensionExtensions
    {
        public static void SetActivity(this DistributedTracingExtension extension, Activity activity)
        {
            extension.TraceParent = activity.Id;
            if (!string.IsNullOrWhiteSpace(activity.TraceStateString))
            {
                extension.TraceState = activity.TraceStateString;
            }
        }

        public static ActivityContext GetActivityContext(this DistributedTracingExtension extension)
        {
            return ActivityContext.Parse(extension.TraceParent, extension.TraceState);
        }
    }
}
