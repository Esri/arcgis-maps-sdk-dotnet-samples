// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Shared.Models;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGIS.Samples.Managers
{
    public static class DataManager
    {
        private static Task DownloadItem(PortalItem item, Action<ProgressInfo> onProgress = null)
        {
            return DownloadItem(item, CancellationToken.None, onProgress);
        }

        /// <summary>
        /// Downloads a portal item and leaves a marker to track download date.
        /// </summary>
        /// <param name="item">Portal item to download.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private static async Task DownloadItem(PortalItem item, CancellationToken cancellationToken, Action<ProgressInfo> onProgress = null)
        {
            // Get sample data directory.
            string dataDir = Path.Combine(GetDataFolder(), item.ItemId);

            // Create directory matching item id.
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            // Get the path to the destination file.
            string tempFile = Path.Combine(dataDir, item.Name);

            // Download the file.
            var downloadTask = await FileDownloadTask.StartDownload(tempFile, item);

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => CancelDownload(downloadTask));
            }
            if (onProgress != null)
            {
                downloadTask.Progress += (s, e) => onProgress(e);
            }

            await downloadTask.DownloadAsync();

            // Verify download wasn't cancelled.
            if (!cancellationToken.IsCancellationRequested)
            {
                // Unzip the file if it is a zip archive.
                if (tempFile.EndsWith(".zip"))
                {
                    await UnpackData(tempFile, dataDir, cancellationToken);
                }

                // Write the __sample.config file. This is used to ensure that cached data did not go out-of-date.
                string configFilePath = Path.Combine(dataDir, "__sample.config");
                File.WriteAllText(configFilePath, @"Data downloaded: " + DateTime.Now);
            }
        }

        private static void CancelDownload(FileDownloadTask downloadTask)
        {
            try
            {
                downloadTask.CancelAsync();
            }
            catch { }
        }

        private static async Task<bool> IsDataPresent(string itemId)
        {
            // Look for __sample.config file. Return false if not present.
            string configPath = Path.Combine(GetDataFolder(itemId), "__sample.config");
            if (!File.Exists(configPath)) { return false; }

            // Get the last write date from the __sample.config file metadata.
            DateTime downloadDate = File.GetLastWriteTime(configPath);

            try
            {
                // Create ArcGIS portal item
                var portal = await ArcGISPortal.CreateAsync().ConfigureAwait(false);
                var item = await PortalItem.CreateAsync(portal, itemId).ConfigureAwait(false);
                // Return true if the item was downloaded after it was last modified.
                return downloadDate >= item.Modified;
            }
            // Catch exception when data manager cant access the internet.
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return true;
            }
        }

        public static async Task EnsureSampleDataPresent(SampleInfo info, Action<ProgressInfo> onProgress = null)
        {
            await EnsureSampleDataPresent(info, CancellationToken.None, onProgress);
        }

        /// <summary>
        /// Ensures that data needed for a sample has been downloaded and is up-to-date.
        /// </summary>
        /// <param name="sample">The sample to ensure data is present for.</param>
        public static Task EnsureSampleDataPresent(SampleInfo sample, CancellationToken token, Action<ProgressInfo> onProgress = null)
        {
            return EnsureSampleDataPresent(sample.OfflineDataItems, token, onProgress);
        }

        public static async Task<bool> HasSampleDataPresent(SampleInfo info)
        {
            if (info.OfflineDataItems is null) return true;
            foreach (string itemId in info.OfflineDataItems)
            {
                bool isDownloaded = await IsDataPresent(itemId);
                if (!isDownloaded) return false;
            }
            return true;
        }

        public static async Task EnsureSampleDataPresent(IEnumerable<string> itemIds, CancellationToken token, Action<ProgressInfo> onProgress = null)
        {
            // Return if there's nothing to do.
            if (itemIds == null || !itemIds.Any()) { return; }

            // Hold a list of download tasks (to enable parallel download).
            List<Task> downloads = new List<Task>();
            Action<ProgressInfo, int> combinedProgress = null;
            if (onProgress != null)
            {
                var count = itemIds.Count();
                ProgressInfo[] totalProgress = new ProgressInfo[count];
                int total = count * 100;
                combinedProgress = (info, idNum) =>
                {
                    totalProgress[idNum] = info;
                    onProgress(new ProgressInfo() { TotalBytes = totalProgress.Sum(t => t is null ? 0 : t.TotalBytes), TotalLength = totalProgress.Sum(t => t is null ? 0 : t.TotalLength) });
                };
            }

            int id = 0;
            foreach (string itemId in itemIds)
            {
                bool isDownloaded = await IsDataPresent(itemId);
                // Download item if not already present
                if (!isDownloaded)
                {
                    // Create ArcGIS portal item
                    var portal = await ArcGISPortal.CreateAsync(token).ConfigureAwait(false);
                    var item = await PortalItem.CreateAsync(portal, itemId, token).ConfigureAwait(false);

                    var index = id;
                    Action<ProgressInfo> action = (info) => combinedProgress(info, index);
                    Task downloadTask = DownloadItem(item, token, combinedProgress is null ? null : action);
                    downloads.Add(downloadTask);
                }
                id++;
            }
            // Wait for all downloads to complete
            await Task.WhenAll(downloads).WithCancellation(token);
        }

        public static Task DownloadDataItem(string itemId)
        {
            return DownloadDataItem(itemId, CancellationToken.None);
        }

        public static async Task DownloadDataItem(string itemId, CancellationToken cancellationToken, Action<ProgressInfo> onProgress = null)
        {
            bool isDownloaded = await IsDataPresent(itemId);
            // Download item if not already present
            if (!isDownloaded)
            {
                // Create ArcGIS portal item
                var portal = await ArcGISPortal.CreateAsync(cancellationToken).ConfigureAwait(false);
                var item = await PortalItem.CreateAsync(portal, itemId, cancellationToken).ConfigureAwait(false);

                await DownloadItem(item, cancellationToken, onProgress);
            }
        }

        public static async Task WithCancellation(this Task baseTask, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (token.Register(
                s =>
                {
                    ((TaskCompletionSource<bool>)s).TrySetResult(true);
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
        public static string GetDataFolder()
        {
#if NETFX_CORE
            string appDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif MACCATALYST
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#elif MAUI
            string appDataFolder = FileSystem.Current.AppDataDirectory;
#else
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
            string sampleDataFolder = Path.Combine(appDataFolder, "ESRI", "dotnetSamples", "Data");

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