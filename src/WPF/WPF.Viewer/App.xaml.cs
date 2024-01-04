// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

#if ENABLE_ANALYTICS
using ArcGIS.Helpers;
#endif

using ArcGIS.Samples.Shared.Managers;
using Esri.ArcGISRuntime;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ArcGIS.WPF.Viewer
{
    public partial class App
    {
        public static string ResourcePath => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Set the local data path - must be done before starting. On most systems, this will be C:\EsriSamples\Temp.
                // This path should be kept short to avoid Windows path length limitations.
                string tempDataPathRoot = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).FullName;
                string tempDataPath = Path.Combine(tempDataPathRoot, "EsriSamples", "Temp");
                Directory.CreateDirectory(tempDataPath); // CreateDirectory won't overwrite if it already exists.
                ArcGISRuntimeEnvironment.TempPath = tempDataPath;

                // Initialize ArcGISRuntime.
                ArcGISRuntimeEnvironment.Initialize((config) =>
                {
                    // Set the licenses for the sample viewer if available.
                    if (!string.IsNullOrEmpty(LicenseStrings.ArcGISLicenseKey))
                        config.UseLicense(LicenseStrings.ArcGISLicenseKey, LicenseStrings.ExtensionLicenses);
                });

#if ENABLE_ANALYTICS
                // Analytics are only used in the Microsoft store version of the viewer.
                AnalyticsHelper.StartAnalytics("", "");
#endif
            }
            catch (Exception ex)
            {
                // Show the message and shut down
                MessageBox.Show(string.Format("There was an error that prevented initializing the runtime. {0}", ex.Message));
                Current.Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
#if ENABLE_ANALYTICS
            // Analytics are only used in the Microsoft store version of the viewer.
            AnalyticsHelper.WriteToSettingsFile();
            _ = AnalyticsHelper.TrackEvent("sample_viewer_closed");
#endif
        }
    }
}