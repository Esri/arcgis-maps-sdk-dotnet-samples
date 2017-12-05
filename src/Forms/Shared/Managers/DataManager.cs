// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Esri.ArcGISRuntime.Portal;
#if NETFX_CORE
using Windows.Storage;

#endif


namespace ArcGISRuntimeXamarin.Managers
{
    public class DataManager
    {
        private DataManager() { }

        private static readonly DataManager SingleInstance = new DataManager();

        /// <summary>
        /// Downloads data from ArcGIS Portal and unzips it to the local data folder
        /// </summary>
        /// <param name="path">The path to put the data in - if the folder already exists, the data is assumed to have been downloaded and this immediately returns</param>
        /// <returns></returns>
        public static async Task GetData(string itemId, string sampleName)
        {
            // Method for creating directories as needed
            Action<DirectoryInfo> createDir = null;
            createDir = (s) =>
            {
                System.Diagnostics.Debug.WriteLine(s.FullName);
                if (Directory.Exists(s.FullName)) return;
                if (!Directory.Exists(s.Parent.FullName))
                    createDir(s.Parent);
                Directory.CreateDirectory(s.FullName);
            };

            // Create the portal
            var portal = await ArcGISPortal.CreateAsync().ConfigureAwait(false);

            // Create the portal item
            var item = await PortalItem.CreateAsync(portal, itemId).ConfigureAwait(false);

            // Create the SampleData folder
            var tempFile = Path.Combine(GetDataFolder(), "SampleData");
            createDir(new DirectoryInfo(tempFile));

            // Create the sample-specific folder
            tempFile = Path.Combine(tempFile, sampleName);
            createDir(new DirectoryInfo(tempFile));

            // Get the full path to the specific file
            tempFile = Path.Combine(tempFile, item.Name);

            // Download the file
            using (var s = await item.GetDataAsync().ConfigureAwait(false))
            {
                using (var f = File.Create(tempFile))
                {
                    await s.CopyToAsync(f).ConfigureAwait(false);
                }
            }

            // Unzip the file if it is a zip file
            if (tempFile.EndsWith(".zip"))
            {
                await UnpackData(tempFile, Path.Combine(GetDataFolder(), "SampleData", sampleName));
            }
        }

        private static async Task UnpackData(string zipFile, string folder)
        {
            using (var archive = ZipFile.OpenRead(zipFile))
            {
                foreach(var entry in archive.Entries.Where(m => !String.IsNullOrWhiteSpace(m.Name)))
                {
                    var path = Path.Combine(folder, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    entry.ExtractToFile(path, true);
                }
            }
        }

        /// <summary>
        /// Gets the data folder where locally provisioned data is stored
        /// </summary>
        /// <returns></returns>
        internal static string GetDataFolder()
        {
            var appDataFolder =
#if NETFX_CORE
                Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif __ANDROID__
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
#elif __IOS__
                "Library/";
#endif
            return appDataFolder;
        }
    }
}


