using ArcGIS.Helpers;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace ArcGIS.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private const string DefaultCategory = "Featured";
        private double _sampleImageWidth;
        private double _sampleImageHeight;
        private double _sampleImageMargin;
        public double SampleImageWidth => _sampleImageWidth;
        public double SampleImageMargin => _sampleImageMargin;

        public CategoryViewModel()
        {
            // Calculate the sample image width and height on mobile platforms based on device display size. 
            _sampleImageWidth = 400;

            // Assign a margin, used to calculate the number of samples that will fit on each row depending on device orientation.
            _sampleImageMargin = 4;

#if WINDOWS

            _sampleImageWidth  = 350;

#elif IOS || ANDROID
            double displayWidth;
            double displayHeight;
            
            // Ensure that on tablet a 3 column grid displays in horizontal orientation and 2 column in vertical orientation.
            // Ensure that on mobile a 2 column grid displays in horizontal orientation and 1 column in vertical orientation.

            if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            {
                displayWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                displayHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            }
            else
            {
                displayWidth = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
                displayHeight = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            }

            // Ensure samples display correctly on devices with varying dimensions.
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                _sampleImageWidth = (displayWidth - 4 * _sampleImageMargin) / 2;

                if (3 * _sampleImageWidth > displayHeight)
                {
                    _sampleImageWidth = (displayHeight - 6 * _sampleImageMargin) / 3;
                }
            }
            else
            {
                _sampleImageWidth = (displayWidth - 2 * _sampleImageMargin);

                if (2 * _sampleImageWidth > displayHeight)
                {
                    _sampleImageWidth = (displayHeight - 4 * _sampleImageMargin) / 2;
                }
            }
#endif
            // Maintain 4:3 image resolution. 
            _sampleImageHeight = Math.Floor(_sampleImageWidth * 3 / 4);

            var featuredSamples = GetSamplesInCategory(DefaultCategory);

            foreach (var sampleInfo in featuredSamples)
            {
                SamplesItems.Add(new SampleViewModel(sampleInfo, _sampleImageWidth, _sampleImageHeight, _sampleImageMargin));
            }

            _selectedCategory = DefaultCategory;

            WeakReferenceMessenger.Default.Register<string>(this, (message, category) => UpdateCategory(category));
        }

        private void UpdateCategory(string category)
        {
            // Close flyout when category changes.
            Shell.Current.FlyoutIsPresented = false;

            SelectedCategory = category;

            var samples = GetSamplesInCategory(category);
            var samplesCollection = samples.Select(s => new SampleViewModel(s, _sampleImageWidth, _sampleImageHeight, _sampleImageMargin)).ToObservableCollection();
            SamplesItems = samplesCollection;
        }

        private static List<SampleInfo> GetSamplesInCategory(string category)
        {
            var categoryNode = SampleManager.Current.FullTree.Items.OfType<SearchableTreeNode>().FirstOrDefault(c => c.Name == category);

            return categoryNode.Items.OfType<SampleInfo>().ToList();
        }

        [ObservableProperty]
        private ObservableCollection<SampleViewModel> _samplesItems = new ObservableCollection<SampleViewModel>();

        [ObservableProperty]
        private string _selectedCategory;

        [RelayCommand]
        void UpdateFavorite(string sampleFormalName)
        {
            var sample = SamplesItems.FirstOrDefault(sample => sample.SampleFormalName == sampleFormalName);

            sample.IsFavorite = !sample.IsFavorite;

            SampleManager.Current.AddRemoveFavorite(sampleFormalName);

            if (SelectedCategory == "Favorites" && SamplesItems.Contains(sample))
            {
                SamplesItems.Remove(sample);
            }
        }

        [RelayCommand]
        static void SampleSelected(SampleViewModel sampleViewModel)
        {
            if(SampleManager.Current.SelectedSample == null)
                _ = SampleLoader.LoadSample(sampleViewModel.SampleObject);
        }
    }

    public partial class SampleViewModel : ObservableObject
    {
        public SampleViewModel(SampleInfo sampleInfo, double sampleImageWidth, double sampleImageHeight, double sampleImageMargin)
        {
            SampleObject = sampleInfo;
            SampleName = sampleInfo.SampleName;
            SampleFormalName = sampleInfo.FormalName;
            Description = sampleInfo.Description;
            SampleImageName = sampleInfo.SampleImageName;
            IsFavorite = sampleInfo.IsFavorite;
            ShowFavoriteIcon = sampleInfo.ShowFavoriteIcon;
            SampleImageWidth = sampleImageWidth;
            SampleImageHeight = sampleImageHeight;
            SampleImageMargin = sampleImageMargin;
        }

        [ObservableProperty]
        string _sampleName;

        [ObservableProperty]
        string _sampleFormalName;

        [ObservableProperty]
        string _description;

        [ObservableProperty]
        bool _isFavorite;

        [ObservableProperty]
        bool _showFavoriteIcon;

        [ObservableProperty]
        string _sampleImageName;

        public SampleInfo SampleObject;

        [ObservableProperty]
        double _sampleImageWidth;

        [ObservableProperty]
        double _sampleImageHeight;

        [ObservableProperty]
        double _sampleImageMargin;
    }
}
