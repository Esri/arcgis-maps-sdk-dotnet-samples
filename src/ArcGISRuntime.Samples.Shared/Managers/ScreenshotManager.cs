using ArcGISRuntime.Samples.Shared.Models;
using System;
using System.IO;

namespace ArcGISRuntime.Samples.Shared.Managers
{
    public static class ScreenshotManager
    {
        // Name for file on windows systems.
        private const string _screenshotSettingsFileName = "screenshotSettings";

#if !(__IOS__ || XAMARIN || __ANDROID__ || WINDOWS_UWP)
        private static ScreenshotSettings _screenshotSettings = GetScreenshotSettings();
        public static ScreenshotSettings ScreenshotSettings { get { return _screenshotSettings; } }

        private static ScreenshotSettings GetScreenshotSettings()
        {
            _screenshotSettings = new ScreenshotSettings() { ScreenshotEnabled = false, SourcePath = string.Empty };

            // Get the screenshot settings from the saved file if it exists.
            // If the file does not exist, create it. 
            if (File.Exists(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName)))
            {
                string settings = File.ReadAllText(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName));

                _screenshotSettings = System.Text.Json.JsonSerializer.Deserialize(settings, typeof(ScreenshotSettings)) as ScreenshotSettings;
            }
            else
            {
                string settings = System.Text.Json.JsonSerializer.Serialize(_screenshotSettings, typeof(ScreenshotSettings));
                File.WriteAllText(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName), settings);
            }

            return _screenshotSettings;
        }

        public static void SaveScreenshotSettings(ScreenshotSettings screenshotSettings)
        {
            _screenshotSettings = screenshotSettings;

            string settings = System.Text.Json.JsonSerializer.Serialize(_screenshotSettings, typeof(ScreenshotSettings));
            File.WriteAllText(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName), settings);
        }

        internal static string GetScreenshotSettingsFolder()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string screenshotSettingsFolder = Path.Combine(appDataFolder, "ArcGISRuntimeScreenshots");

            if (!Directory.Exists(screenshotSettingsFolder)) { Directory.CreateDirectory(screenshotSettingsFolder); }

            return screenshotSettingsFolder;
        }
#endif
    }
}
