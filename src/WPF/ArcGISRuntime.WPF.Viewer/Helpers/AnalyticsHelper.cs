using Microsoft.AppCenter.Analytics;
using System.Collections.Generic;

namespace ArcGISRuntime.Helpers
{
    internal class AnalyticsHelper
    {
        public static bool AnalyticsEnabled;

        // Helper method to only track events when analytics is actually running. This prevents unneeded debug output.
        public static void TrackEvent(string name, IDictionary<string, string> properties = null)
        {
            if (!AnalyticsEnabled) return;

            Analytics.TrackEvent(name, properties);
        }
    }
}