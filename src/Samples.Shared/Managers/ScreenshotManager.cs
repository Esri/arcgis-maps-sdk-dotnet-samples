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

#if !(WINDOWS_UWP)
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

#if NETFRAMEWORK
                    _screenshotSettings = DeserializeScreenshotSettingsJson(settings);
#else
                    _screenshotSettings = System.Text.Json.JsonSerializer.Deserialize(settings, typeof(ScreenshotSettings)) as ScreenshotSettings;
#endif
                }
                else
                {
                    string settings;
#if NETFRAMEWORK
                    settings = SerializeScreenshotSettings(_screenshotSettings);
#else
                    settings = System.Text.Json.JsonSerializer.Serialize(_screenshotSettings, typeof(ScreenshotSettings));
#endif
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
#if NETFRAMEWORK
                settings = SerializeScreenshotSettings(screenshotSettings);
#else
                settings = System.Text.Json.JsonSerializer.Serialize(_screenshotSettings, typeof(ScreenshotSettings));
#endif
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

#if NETFRAMEWORK
        public static string SerializeScreenshotSettings(ScreenshotSettings screenshotSettings)
        {
            // Create a stream to serialize the object to.
            var ms = new MemoryStream();

            var ser = new DataContractJsonSerializer(typeof(ScreenshotSettings));
            ser.WriteObject(ms, screenshotSettings);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        public static ScreenshotSettings DeserializeScreenshotSettingsJson(string json)
        {
            var deserializedUser = new ScreenshotSettings();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var ser = new DataContractJsonSerializer(deserializedUser.GetType());
            deserializedUser = ser.ReadObject(ms) as ScreenshotSettings;
            ms.Close();
            return deserializedUser;
        }
#endif
#endif
    }
}