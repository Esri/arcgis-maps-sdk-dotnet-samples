﻿// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.
#if !_SSG_TOOLING_
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.Managers
{
    public static class DataManager
    {
        private static Task DownloadItem(PortalItem item)
        {
            return DownloadItem(item, CancellationToken.None);
        }

        /// <summary>
        /// Downloads a portal item and leaves a marker to track download date.
        /// </summary>
        /// <param name="item">Portal item to download.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private static async Task DownloadItem(PortalItem item, CancellationToken cancellationToken)
        {
            // Get sample data directory.
            string dataDir = Path.Combine(GetDataFolder(), item.ItemId);

            // Create directory matching item id.
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            // Get the download task.
            Task<Stream> downloadTask = item.GetDataAsync(cancellationToken);

            // Get the path to the destination file.
            string tempFile = Path.Combine(dataDir, item.Name);

            // Download the file.
            using (var s = await downloadTask.ConfigureAwait(false))
            {
                using (var f = File.Create(tempFile))
                {
                    await s.CopyToAsync(f).WithCancellation(cancellationToken).ConfigureAwait(false);
                }
            }

            // Unzip the file if it is a zip archive.
            if (tempFile.EndsWith(".zip"))
            {
                await UnpackData(tempFile, dataDir, cancellationToken);
            }

            // Write the __sample.config file. This is used to ensure that cached data did not go out-of-date.
            string configFilePath = Path.Combine(dataDir, "__sample.config");
            File.WriteAllText(configFilePath, @"Data downloaded: " + DateTime.Now);
        }

        /// <summary>
        /// Determines if a portal item has been downloaded and is up-to-date. 
        /// </summary>
        /// <param name="item">The portal item to check.</param>
        /// <returns><c>true</c> if data is available and up-to-date, false otherwise.</returns>
        private static bool IsDataPresent(PortalItem item)
        {
            // Look for __sample.config file. Return false if not present.
            string configPath = Path.Combine(GetDataFolder(item.ItemId), "__sample.config");
            if (!File.Exists(configPath)) { return false; }

            // Get the last write date from the __sample.config file metadata.
            DateTime downloadDate = File.GetLastWriteTime(configPath);

            // Return true if the item was downloaded after it was last modified.
            return downloadDate >= item.Modified;
        }

        public static async Task EnsureSampleDataPresent(SampleInfo info)
        {
            await EnsureSampleDataPresent(info, CancellationToken.None);
        }

        /// <summary>
        /// Ensures that data needed for a sample has been downloaded and is up-to-date.
        /// </summary>
        /// <param name="sample">The sample to ensure data is present for.</param>
        public static async Task EnsureSampleDataPresent(SampleInfo sample, CancellationToken token)
        {
            // Return if there's nothing to do.
            if (sample.OfflineDataItems == null || !sample.OfflineDataItems.Any()) { return; }

            // Hold a list of download tasks (to enable parallel download).
            List<Task> downloads = new List<Task>();

            foreach (string itemId in sample.OfflineDataItems)
            {
                // Create ArcGIS portal item
                var portal = await ArcGISPortal.CreateAsync(token).ConfigureAwait(false);
                var item = await PortalItem.CreateAsync(portal, itemId, token).ConfigureAwait(false);
                // Download item if not already present
                if (!IsDataPresent(item))
                {
                    Task downloadTask = DownloadItem(item, token);
                    downloads.Add(downloadTask);
                }
            }
            // Wait for all downloads to complete
            await Task.WhenAll(downloads).WithCancellation(token);
        }

        public static Task DownloadDataItem(string itemId)
        {
            return DownloadDataItem(itemId, CancellationToken.None);
        }

        public static async Task DownloadDataItem(string itemId, CancellationToken cancellationToken)
        {
            // Create ArcGIS portal item
            var portal = await ArcGISPortal.CreateAsync(cancellationToken).ConfigureAwait(false);
            var item = await PortalItem.CreateAsync(portal, itemId, cancellationToken).ConfigureAwait(false);
            // Download item if not already present
            if (!IsDataPresent(item))
            {
                await DownloadItem(item, cancellationToken);
            }
        }

        public static async Task WithCancellation(this Task baseTask, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (token.Register(
                s =>
                {
                    ((TaskCompletionSource<bool>) s).TrySetResult(true);
                }, tcs))
            if (baseTask != await Task.WhenAny(baseTask, tcs.Task)) 
                throw new OperationCanceledException(token); 
        }

        private static async Task UnpackData(string zipFile, string folder)
        {
            await UnpackData(zipFile, folder, CancellationToken.None);
        }

        /// <summary>
        /// Unzips the file at path defined by <paramref name="zipFile"/>
        ///  into <paramref name="folder"/>.
        /// </summary>
        /// <param name="zipFile">Path to the zip archive to extract.</param>
        /// <param name="folder">Destination folder.</param>
        private static Task UnpackData(string zipFile, string folder, CancellationToken token)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                using (var archive = ZipFile.OpenRead(zipFile))
                {
                    foreach (var entry in archive.Entries.Where(m => !String.IsNullOrWhiteSpace(m.Name)))
                    {
                        string path = Path.Combine(folder, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        entry.ExtractToFile(path, true);
                    }
                }
            }, token).WithCancellation(token);
        }

        /// <summary>
        /// Gets the data folder where locally provisioned data is stored.
        /// </summary>
        internal static string GetDataFolder()
        {
#if NETFX_CORE
            string appDataFolder  = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif XAMARIN
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
            string sampleDataFolder = Path.Combine(appDataFolder, "ArcGISRuntimeSampleData");

            if (!Directory.Exists(sampleDataFolder)) { Directory.CreateDirectory(sampleDataFolder); }

            return sampleDataFolder;
        }

        /// <summary>
        /// Gets the path to an item on disk. 
        /// The item must have already been downloaded for the path to be valid.
        /// </summary>
        /// <param name="itemId">ID of the portal item.</param>
        internal static string GetDataFolder(string itemId)
        {
            return Path.Combine(GetDataFolder(), itemId);
        }

        /// <summary>
        /// Gets the path to an item on disk. 
        /// The item must have already been downloaded for the path to be valid.
        /// </summary>
        /// <param name="itemId">ID of the portal item.</param>
        /// <param name="pathParts">Components of the path.</param>
        internal static string GetDataFolder(string itemId, params string[] pathParts)
        {
            return Path.Combine(GetDataFolder(itemId), Path.Combine(pathParts));
        }
    }
}
#endif
