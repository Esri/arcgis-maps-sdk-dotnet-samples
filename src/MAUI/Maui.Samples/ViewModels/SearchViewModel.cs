using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace ArcGIS.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        private IDispatcherTimer _delaySearchTimer;
        private const int _delayedTextChangedTimeout = 500;

        public SearchViewModel()
        {
            _searchItems = new ObservableCollection<SearchResultViewModel>();
        }

        [ObservableProperty]
        ObservableCollection<SearchResultViewModel> _searchItems;

        [ObservableProperty]
        string _searchText;

        [RelayCommand]
        void PerformSearch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                SearchItems = new ObservableCollection<SearchResultViewModel>();
            }
            else
            {
                var allSamples = SampleManager.Current.AllSamples.ToList();
                string searchText = searchTerm.ToLower();

                var sampleResults = allSamples.Where(sample => sample.SampleName.ToLower().Contains(searchText) ||
                   sample.Category.ToLower().Contains(searchText) ||
                   sample.Description.ToLower().Contains(searchText) ||
                   sample.Tags.Any(tag => tag.Contains(searchText))).ToList<SampleInfo>();

                try
                {
                    var searchResults = new ObservableCollection<SearchResultViewModel>();

                    SearchItems.Clear();

                    foreach (var sampleResult in sampleResults)
                    {
                        SearchItems.Add(new SearchResultViewModel(sampleResult));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SearchText))
            {
                SearchTextChanged();
            }
        }

        private void SearchTextChanged()
        {
            // Don't update results immediately; makes search-as-you-type more comfortable
            if (_delaySearchTimer != null)
            {
                _delaySearchTimer.Stop();
            }

            if (_delaySearchTimer == null)
            {
                _delaySearchTimer = Application.Current.Dispatcher.CreateTimer();
                _delaySearchTimer.Tick += DelaySearchTimer_Tick;
                _delaySearchTimer.Interval = TimeSpan.FromMilliseconds(_delayedTextChangedTimeout);
            }

            _delaySearchTimer.Start();
        }

        private void DelaySearchTimer_Tick(object sender, EventArgs e)
        {
            PerformSearch(SearchText);

            if (_delaySearchTimer != null)
            {
                _delaySearchTimer.Stop();
            }
        }
    }

    public partial class SearchResultViewModel : ObservableObject
    {
        public SearchResultViewModel(SampleInfo sampleResult)
        {
            SampleName = sampleResult.SampleName;
            SampleImage = GetImageSourceBySampleName(sampleResult.FormalName);
            SampleObject = sampleResult;
        }

        [ObservableProperty]
        string _sampleName;

        [ObservableProperty]
        ImageSource _sampleImage;

        [ObservableProperty]
        SampleInfo _sampleObject;

        private ImageSource GetImageSourceBySampleName(string sampleFormalName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string imageResource = assembly.GetManifestResourceNames().Single(n => n.EndsWith($"{sampleFormalName}.jpg"));
            var sourceStream = assembly.GetManifestResourceStream(imageResource);
            var memoryStream = new MemoryStream();
            sourceStream.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            memoryStream.Close();

            var image = ImageSource.FromStream(() =>
            {
                return new MemoryStream(bytes);
            });

            return image;
        }
    }
}
