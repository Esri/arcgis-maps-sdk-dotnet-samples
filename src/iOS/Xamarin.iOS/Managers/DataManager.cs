// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.IO;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;


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
            var portal = await ArcGISPortal.CreateAsync().ConfigureAwait(false);
            var item = await PortalItem.CreateAsync(portal, itemId).ConfigureAwait(false);  
            var tempFile = Path.GetTempFileName();

            using (var s = await item.GetDataAsync().ConfigureAwait(false))
            {
                using (var f = File.Create(tempFile))
                {
                    await s.CopyToAsync(f).ConfigureAwait(false);
                }
            }      
            await UnpackData(tempFile, Path.Combine(GetDataFolder(),"SampleData", sampleName));
        }

        private static async Task UnpackData(string zipFile, string folder)
        {
            await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, folder));
        }

        /// <summary>
        /// Gets the data folder where locally provisioned data is stored
        /// </summary>
        /// <returns></returns>
        internal static string GetDataFolder()
        {
            var appDataFolder = "Library/";
            return appDataFolder;
        }
    }
}


