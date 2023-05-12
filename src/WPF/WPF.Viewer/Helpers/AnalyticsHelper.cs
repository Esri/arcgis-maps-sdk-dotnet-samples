#if ENABLE_ANALYTICS

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArcGIS.Helpers
{
    internal class AnalyticsHelper
    {
        // Property to determine telemetry tab visibility.
        // This is only visible in the store version of the WPF viewer.
        public static bool AnalyticsStarted;

        // Property that dictates whether or not telemetry data is gathered.
        public static bool AnalyticsEnabled;

        // Variables set when the analytics process starts, used for HTTP requests.
        private static Guid _clientId;
        private static long _sessionStartTime;
        private static string _api_secret;
        private static string _measurement_id;
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Initialize Google Analytics settings. 
        /// </summary>
        /// <param name="apiSecret">The required API key for Google Analytics.</param>
        /// <param name="measurementId">The required measurement ID for Google Analytics.</param>
        public static void StartAnalytics(string apiSecret, string measurementId)
        {
            _api_secret = apiSecret;
            _measurement_id = measurementId;
            _sessionStartTime = DateTime.UtcNow.Ticks;
            AnalyticsStarted = true;
        }

        /// <summary>
        /// Helper method to only track events when analytics is actually running. This prevents unneeded debug output.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventData"></param>
        public static async Task TrackEvent(string eventName, IDictionary<string, string> eventData = null)
        {
            if (!AnalyticsEnabled) return;

            try
            {
                if (eventData == null)
                {
                    eventData = new Dictionary<string, string>();
                }

                double sessionCurrentTimeMs = new TimeSpan(DateTime.UtcNow.Ticks - _sessionStartTime).TotalMilliseconds;
                eventData.Add("engagement_time_msec", sessionCurrentTimeMs.ToString());

                var events = new Dictionary<string, object>()
                {
                    {"name", eventName},
                    {"params", eventData }
                };

                // The request body including the required client ID field and an array of tracked events.
                var postData = new Dictionary<string, object>
                       {
                           { "client_id", _clientId },
                           { "events", events }
                       };

                // Serialize json body content.
                var jsonContent = JsonSerializer.Serialize(postData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Initialize Google Analytics URI.
                var uri = new Uri("https://google-analytics.com/mp/collect?api_secret=" + _api_secret + "&measurement_id=" + _measurement_id);

                // Send HTTP Request.
                HttpResponseMessage response = await _httpClient.PostAsync(uri, content);
            }
            catch
            {
                // Silently fail.
            }
        }

        #region Save/Load Analytics Settings
        public static void WriteToSettingsFile()
        {
            // Create an object for storing the analytics settings.
            AnalyticsSettings analyticsSettings = new AnalyticsSettings()
            {
                ClientId = _clientId,
                AnalyticsEnabled = AnalyticsEnabled,
            };

            // Save the settings to a local file.
            string filePath = GetLocalFilePath("settings");
            string jsonSettings = JsonSerializer.Serialize(analyticsSettings);
            File.WriteAllText(filePath, jsonSettings);
        }

        public static void ReadSettingsFromFile()
        {
            // Initialize a new object for the analytics settings.
            AnalyticsSettings analyticsSettings = new AnalyticsSettings();

            // Read client id and analytics enabled settings from local file.
            string settingsPath = GetLocalFilePath("settings");

            if (File.Exists(settingsPath))
            {
                analyticsSettings = JsonSerializer.Deserialize<AnalyticsSettings>(File.ReadAllText(settingsPath));
            }

            // Generate a client id if one does not already exist, enable analytics by default.
            _clientId = analyticsSettings.ClientId.HasValue ? analyticsSettings.ClientId.Value : Guid.NewGuid();
            AnalyticsEnabled = analyticsSettings.ClientId.HasValue ? analyticsSettings.AnalyticsEnabled : true;
        }

        private static string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, fileName);
        }
        #endregion
    }
}
#endif