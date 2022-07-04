using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.Managers
{
    // Helper classes for managing and resuming downloads

    public class ProgressInfo
    {
        public bool HasPercentage => TotalLength.HasValue && TotalLength.Value > 0;
        public int Percentage => HasPercentage ? (int)Math.Round(TotalBytes * 100d / TotalLength.Value, 0) : 0;
        public long TotalBytes { get; internal set; }
        public long? TotalLength { get; internal set; } = null;
    }

    public enum DownloadStatus
    {
        Downloading,
        Paused,
        Resuming,
        Completed,
        Error,
        Cancelled
    }

    [DataContract]
    public class FileDownloadTask
    {
        private HttpResponseMessage content;
        private HttpClient client;
        private CancellationTokenSource cancellationSource;
        private Task transferTask;

        private FileDownloadTask(string filename, Uri requestUri, HttpResponseMessage content, HttpClient client)
        {
            RequestUri = requestUri;
            this.client = client;
            this.content = content;
            this.Filename = filename;
            transferTask = BeginTransfer(content);
        }

        public Exception Exception { get; private set; }
        [DataMember]
        public DownloadStatus Status { get; internal set; }
        [DataMember]
        public int BufferSize { get; set; } = 65535;

        [DataMember]
        internal string ETag { get; set; }

        [DataMember]
        internal DateTimeOffset? Date { get; set; }

        internal bool IsResumable { get; set; }

        [DataMember]
        internal Uri RequestUri { get; set; }

        [DataMember]
        internal string Filename { get; set; }

        public long BytesDownloaded { get; private set; }

        [DataMember]
        public long? TotalLength { get; internal set; }

        public event EventHandler Completed;

        public event EventHandler<ProgressInfo> Progress;

        public event EventHandler<Exception> Error;

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            if (Status == DownloadStatus.Downloading || Status == DownloadStatus.Resuming)
                Status = DownloadStatus.Paused;
            var fi = new FileInfo(Filename);

            if (fi.Exists)
            {
                IsResumable = !string.IsNullOrEmpty(ETag) || Date.HasValue;
                BytesDownloaded = fi.Length;
            }
        }

        public static FileDownloadTask FromJson(string json, HttpMessageHandler handler = null)
        {
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(FileDownloadTask));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var fdt = s.ReadObject(ms) as FileDownloadTask;
                fdt.client = new HttpClient(handler ?? new HttpClientHandler());
                return fdt;
            }
        }

        public string ToJson()
        {
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(FileDownloadTask));
            using (var ms = new MemoryStream())
            {
                s.WriteObject(ms, this);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static Task<FileDownloadTask> StartDownload(string filename, Esri.ArcGISRuntime.Portal.PortalItem portalItem, HttpMessageHandler handler = null)
        {
            var uri1 = portalItem.Portal.Uri;
            string requestUri = $"http://www.arcgis.com/sharing/rest/content/items/{portalItem.ItemId}/data";
            return StartDownload(new HttpRequestMessage(HttpMethod.Get, requestUri), filename, handler);
        }

        public static Task<FileDownloadTask> StartDownload(string filename, Uri requestUri, HttpMessageHandler handler = null)
        {
            return StartDownload(new HttpRequestMessage(HttpMethod.Get, requestUri), filename, handler);
        }

        private static async Task<FileDownloadTask> StartDownload(HttpRequestMessage request, string filename, HttpMessageHandler handler = null)
        {
            HttpClient client;
            if (handler == null)
                handler = new HttpClientHandler();
            client = new HttpClient(handler);
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            var content = response.EnsureSuccessStatusCode();

            FileDownloadTask result = new FileDownloadTask(filename, request.RequestUri, content, client);
            return result;
        }

        public async Task RestartAsync()
        {
            var task = transferTask;
            if (task != null && cancellationSource != null)
            {
                try
                {
                    await CancelAsync();
                }
                catch { }
            }
            await BeginDownload(0);
        }

        public Task ResumeAsync()
        {
            if (Status != DownloadStatus.Paused && Status != DownloadStatus.Error)
                throw new InvalidOperationException("Can only resume a paused or failed task");
            if (!IsResumable)
                throw new InvalidOperationException("The server does not support resume");
            Status = DownloadStatus.Resuming;
            FileInfo f = new FileInfo(Filename);
            long offset = f.Exists ? f.Length : 0;
            return BeginDownload(offset);
        }

        public Task CancelAsync()
        {
            var task = transferTask;
            if (Status == DownloadStatus.Paused)
            {
                if (File.Exists(Filename))
                    File.Delete(Filename);
                Status = DownloadStatus.Cancelled;
                return Task.CompletedTask;
            }
            else
            {
                if (cancellationSource == null || task == null)
                    throw new InvalidOperationException("Download not running");
                cancellationSource.Cancel();
                return task.ContinueWith(t => { File.Delete(Filename); Status = DownloadStatus.Cancelled; });
            }
        }

        public Task PauseAsync()
        {
            var task = transferTask;
            if (cancellationSource == null || task == null)
                throw new InvalidOperationException("Download not running");
            cancellationSource.Cancel();
            return task.ContinueWith(t => { Status = DownloadStatus.Paused; });
        }

        /// <summary>
        /// Returns when the download completed (or failed)
        /// </summary>
        /// <returns></returns>
        public async Task DownloadAsync()
        {
            if (Status == DownloadStatus.Paused || Status == DownloadStatus.Error)
                await ResumeAsync().ConfigureAwait(false);
            else if (transferTask == null)
            {
                throw new InvalidOperationException();
            }
            await transferTask;
        }

        private async Task BeginDownload(long offset)
        {
            if (offset == TotalLength)
            {
                transferTask = Task.CompletedTask;
                Status = DownloadStatus.Completed;
                return;
            }
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            if (offset > 0)
            {
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(offset, null);
                if (ETag != null)
                    request.Headers.IfRange = new System.Net.Http.Headers.RangeConditionHeaderValue(ETag);
                else if (Date != null)
                    request.Headers.IfRange = new System.Net.Http.Headers.RangeConditionHeaderValue(Date.Value);
            }
            try
            {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                content = response.EnsureSuccessStatusCode();
                transferTask = BeginTransfer(content);
            }
            catch (System.Exception)
            {
                Status = DownloadStatus.Error;
                throw;
            }
        }

        private async Task BeginTransfer(HttpResponseMessage content)
        {
            Exception = null;
            cancellationSource = new CancellationTokenSource();
            IsResumable = content.Headers.AcceptRanges?.Contains("bytes") == true;
            if (IsResumable)
            {
                ETag = content.Headers.ETag?.Tag;
                Date = content.Headers.Date;
            }
            var token = cancellationSource.Token;
            Status = DownloadStatus.Downloading;
            byte[] buffer = new byte[BufferSize];
            long? length = content.Content.Headers.ContentLength;
            if (!length.HasValue)
                length = content.Content.Headers.ContentDisposition?.Size;
            long position = 0;
            if (content.StatusCode == System.Net.HttpStatusCode.PartialContent)
            {
                if (content.Content.Headers.ContentRange?.HasRange == true)
                {
                    position = content.Content.Headers.ContentRange.From.Value;
                }
                if (content.Content.Headers.ContentRange?.HasLength == true)
                {
                    length = content.Content.Headers.ContentRange.Length;
                }
            }
            TotalLength = length;
            int count = 0;
            try
            {
                using (var readStream = await content.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    using (var file = File.Open(Filename, position == 0 ? FileMode.Create : FileMode.Open, FileAccess.Write))
                    {
                        if (position > 0)
                        {
                            if (file.Length < position)
                                throw new System.IO.IOException("File is smaller than starting position");
                            file.Seek(position, SeekOrigin.Begin);
                            file.SetLength(position);
                        }

                        long total = position;
                        while ((count = await readStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                        {
                            await file.WriteAsync(buffer, 0, count, token).ConfigureAwait(false);
                            await file.FlushAsync(token).ConfigureAwait(false);
                            total += count;
                            BytesDownloaded = total;
                            Progress?.Invoke(this, new ProgressInfo() { TotalBytes = total, TotalLength = length });
                            if (token.IsCancellationRequested)
                            {
                                throw new TaskCanceledException();
                            }
                        }
                        Status = DownloadStatus.Completed;
                        Completed?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (System.Exception error)
            {
                if (!token.IsCancellationRequested)
                {
                    Exception = error;
                    Status = DownloadStatus.Error;
                    Error?.Invoke(this, error);
                }
            }
            finally
            {
                cancellationSource = null;
                transferTask = null;
            }
        }
    }
}