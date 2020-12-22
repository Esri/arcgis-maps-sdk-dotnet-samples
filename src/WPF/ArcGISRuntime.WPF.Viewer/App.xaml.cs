// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.IO;
using System.Windows;

namespace ArcGISRuntime.WPF.Viewer
{
    public partial class App
    {
        public static string ResourcePath => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Set the local data path - must be done before starting. On most systems, this will be C:\EsriSamples\Temp.
                // This path should be kept short to avoid Windows path length limitations.
                string tempDataPathRoot = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).FullName;
                string tempDataPath = Path.Combine(tempDataPathRoot, "EsriSamples", "Temp");
                Directory.CreateDirectory(tempDataPath); // CreateDirectory won't overwrite if it already exists.
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.TempPath = tempDataPath;

                // Initialize ArcGISRuntime.
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();

                
                string apiKey = ApiKeyManager.ArcGISDeveloperApiKey;

                // Check if key is null.
                if (apiKey == null) PromptForKey();
                else
                {
                    try
                    {
                        Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = apiKey;

                        // Check that key is valid.
                        await new Map(BasemapStyle.ArcGISTopographic).LoadAsync();
                    }
                    catch(Exception ex)
                    {
                        if (ex.Message == "The provided APIKey is invalid, expired or does not have access to the resource.") PromptForKey();
                        else
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Show the message and shut down
                MessageBox.Show(string.Format("There was an error that prevented initializing the runtime. {0}", ex.Message));
                Current.Shutdown();
            }
        }
        private void PromptForKey()
        {
            Console.WriteLine("No developer API key set.");
        }
    }

    
}
