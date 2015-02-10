using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace ArcGISRuntime.Samples.PhoneViewer
{
    internal class SampleDataViewModel : INotifyPropertyChanged
    {
		private const string AGOL_SAMPLE_DATA_URL = "http://www.arcgis.com/sharing/content/items/698cf2dd47994e169353bf997b3ab1d8/data";
		private const string NotDownloadedString = "Never";

		// Flag to indicate whether data has been previously downloaded
		public bool HasData
		{
			get { return (!string.IsNullOrEmpty(LastDownloadDate) && (LastDownloadDate != NotDownloadedString)); }
		}

		// String with date of last data download
        private string _lastDownloadDate;
        public string LastDownloadDate
        {
            get { return _lastDownloadDate; }
			set { _lastDownloadDate = value; RaisePropertyChanged(); RaisePropertyChanged("HasData"); }
        }
        
        // ArcGIS.com address to sample data URL
        private string _sampleDataUrl;
        public string SampleDataUrl
        {
            get { return _sampleDataUrl; }
            set { _sampleDataUrl = value; RaisePropertyChanged(); }
        }

        // Returns whether the download has errors
        public bool HasDownloadErrors
        {
            get { return (_downloadErrors != null) && (_downloadErrors.Count > 0); }
        }

        // Download errors
        private ObservableCollection<string> _downloadErrors;
        public ObservableCollection<string> DownloadErrors
        {
            get { return _downloadErrors; }
            private set { _downloadErrors = value; RaisePropertyChanged(); }
        }

        // Flag to indicate whether a download is currently happening
        private bool _isDownloading;
        public bool IsDownloading
        {
            get { return _isDownloading; }
            private set { _isDownloading = value; RaisePropertyChanged(); }
        }

        // Name of the currently executing action (i.e. Download, Extract)
        private string _currentAction;
        public string CurrentAction
        {
            get { return _currentAction; }
            set { _currentAction = value; RaisePropertyChanged(); }
        }

        // Name of file currently being downloaded
        private string _currentFileName;
        public string CurrentFileName
        {
            get { return _currentFileName; }
            set { _currentFileName = value; RaisePropertyChanged();  }
        }

        /// <summary>Initialize SampleDataViewModel class</summary>
        public SampleDataViewModel()
        {
            _sampleDataUrl = AGOL_SAMPLE_DATA_URL;
            _downloadErrors = new ObservableCollection<string>();
            _isDownloading = false;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("LastDataDownloadDate"))
                _lastDownloadDate = ApplicationData.Current.LocalSettings.Values["LastDataDownloadDate"].ToString();
            else
                _lastDownloadDate = NotDownloadedString;
        }

        /// <summary>Downloads a .zip file from online URL and extracts it to the App Local Foilder</summary>
        public async Task DownloadLocalDataAsync()
        {
            try
            {
                IsDownloading = true;
                CurrentAction = CurrentFileName = string.Empty;
				if (HasDownloadErrors)
				{
					DownloadErrors.Clear();
					RaisePropertyChanged("HasDownloadErrors");
				}

                CurrentAction = "Downloading...";
                var client = new HttpClient();
                var response = await client.GetAsync(SampleDataUrl);

                CurrentAction = "Reading Archive...";
                var zipStream = await response.Content.ReadAsStreamAsync();

                CurrentAction = "Extracting Files...";
                await ExtractZipArchive(zipStream, ApplicationData.Current.LocalFolder);

                LastDownloadDate = DateTime.Now.ToString();
                ApplicationData.Current.LocalSettings.Values["LastDataDownloadDate"] = LastDownloadDate;

				CurrentAction = "Data Download Succeeded!";
            }
            catch (Exception ex)
            {
				CurrentAction = "Data Download Failed.";
				DownloadErrors.Add(ex.Message);
                RaisePropertyChanged("HasDownloadErrors");
            }
            finally
            {
                IsDownloading = false;
            }
        }

        // Extract files from a downloaded zip archive stream
        private async Task ExtractZipArchive(Stream zipStream, StorageFolder folder)
        {
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    CurrentFileName = entry.FullName;

                    if (String.IsNullOrEmpty(entry.Name))
                    {
                        string dir = Path.GetDirectoryName(entry.FullName);
                        await folder.CreateFolderAsync(dir, CreationCollisionOption.ReplaceExisting);
                    }
                    else
                    {
                        await ExtractZipEntryAsync(entry, folder);
                    }
                }
            }
        }

        // Extract a single zip archive entry to the given folder
        private async Task ExtractZipEntryAsync(ZipArchiveEntry entry, StorageFolder folder)
        {
            try
            {
                using (Stream entryStream = entry.Open())
                {
                    byte[] buffer = new byte[entry.Length];
                    entryStream.Read(buffer, 0, buffer.Length);

                    string filePath = entry.FullName.Replace('/', '\\');
                    StorageFile uncompressedFile = await folder.CreateFileAsync(filePath, CreationCollisionOption.ReplaceExisting);

                    using (var uncompressedFileStream = await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                        {
                            outstream.Write(buffer, 0, buffer.Length);
                            outstream.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DownloadErrors.Add(ex.ToString());
                RaisePropertyChanged("HasDownloadErrors");
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
