using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;
using System.Globalization;

namespace ArcGISRuntime.Helpers
{
    internal class AnalyticsHelper
    {
        public static bool AnalyticsEnabled;

        /// <summary>
        /// Helper method to only track events when analytics is actually running. This prevents unneeded debug output.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="properties"></param>
        public static void TrackEvent(string name, IDictionary<string, string> properties = null)
        {
            if (!AnalyticsEnabled) return;

            Analytics.TrackEvent(name, properties);
        }

        /// <summary>
        /// Disable analytics without changing the Analytics enabled setting.
        /// </summary>
        public static void DisableAnalytics()
        {
            Analytics.Instance.InstanceEnabled = false;
        }

        /// <summary>
        /// Re-enable if they are enabled in the public property.
        /// </summary>
        public static void EnableAnalytics()
        {
            Analytics.Instance.InstanceEnabled = AnalyticsEnabled;
        }

        /// <summary>
        /// Start analytics for the app.
        /// </summary>
        public static void StartAnalytics(string appSecret)
        {
            AnalyticsEnabled = true;

            // Start app analytics.
            AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));
            AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);
            Analytics.StartSession();
        }

        public static bool AnalyticsStarted => Analytics.Instance.InstanceEnabled;
    }
}