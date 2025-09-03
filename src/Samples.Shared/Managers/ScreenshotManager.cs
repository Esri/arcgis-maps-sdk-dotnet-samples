using ArcGIS.Samples.Shared.Models;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ArcGIS.Samples.Shared.Managers
{
    public static class ScreenshotManager
    {
        // Name for file on windows systems.
        private const string _screenshotSettingsFileName = "screenshotSettings";

        private static ScreenshotSettings _screenshotSettings = GetScreenshotSettings();

        public static ScreenshotSettings ScreenshotSettings
        { get { return _screenshotSettings; } }

        private static ScreenshotSettings GetScreenshotSettings()
        {
            _screenshotSettings = new ScreenshotSettings() { ScreenshotEnabled = false, SourcePath = string.Empty };

            try
            {
                // Get the screenshot settings from the saved file if it exists.
                // If the file does not exist, create it.
                if (File.Exists(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName)))
                {
                    string settings = File.ReadAllText(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName));
                    _screenshotSettings = System.Text.Json.JsonSerializer.Deserialize(settings, typeof(ScreenshotSettings)) as ScreenshotSettings;
                }
                else
                {
                    string settings;
                    settings = System.Text.Json.JsonSerializer.Serialize(_screenshotSettings, typeof(ScreenshotSettings));
                    File.WriteAllText(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName), settings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving screenshot settings: {ex.Message}");
            }

            return _screenshotSettings;
        }

        public static void SaveScreenshotSettings(ScreenshotSettings screenshotSettings)
        {
            _screenshotSettings = screenshotSettings;

            try
            {
                string settings;
                settings = System.Text.Json.JsonSerializer.Serialize(_screenshotSettings, typeof(ScreenshotSettings));
                File.WriteAllText(Path.Combine(GetScreenshotSettingsFolder(), _screenshotSettingsFileName), settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving screenshot settings: {ex.Message}");
            }
        }

        internal static string GetScreenshotSettingsFolder()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string screenshotSettingsFolder = Path.Combine(appDataFolder, "ESRI", "dotnetSamples", "ScreenshotSettings");

            if (!Directory.Exists(screenshotSettingsFolder)) { Directory.CreateDirectory(screenshotSettingsFolder); }

            return screenshotSettingsFolder;
        }
    }
}