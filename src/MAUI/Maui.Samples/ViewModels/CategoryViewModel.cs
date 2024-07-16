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

            // Ensure that the correct dimension is being used, this accounts for situations where the viewer is opened in landscape orientation. 
            var displayWidth = DeviceDisplay.MainDisplayInfo.Width;// == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Width : DeviceDisplay.MainDisplayInfo.Height;

            var displayDensity = DeviceDisplay.MainDisplayInfo.Density;

#if WINDOWS

            _sampleImageWidth  = 300;

#elif IOS || ANDROID

            // Calculate the width for the image using the device display density. Account for a margin around the image. 
            _sampleImageWidth = displayWidth / displayDensity - 20;
            
            // For tablets check to see if multiple images could fit rather than one tablet sized image. 
            // If multiple images of arbitrary size "300" would fit then update the image width.
            var sampleImageFactor = Math.Floor(_sampleImageWidth / 300);

            if (sampleImageFactor > 1)
            {
                _sampleImageWidth = _sampleImageWidth / sampleImageFactor;
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

            Span = (int)Math.Floor((displayWidth / displayDensity) / (_sampleImageWidth + 4 * _sampleImageMargin));

            WeakReferenceMessenger.Default.Register<string>(this, (message, category) => UpdateCategory(category));

#if IOS || ANDROID
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
#endif
        }

#if IOS || ANDROID
        private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            UpdateGridSpan();
        }
#endif

        public void UpdateGridSpan(double? width = null)
        {
            double displayWidth;

            if (width != null)
            {
                displayWidth = width.Value;
            }
            else
            {
                displayWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;// == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Height : DeviceDisplay.MainDisplayInfo.Width;
            }

            Span = (int)Math.Floor(displayWidth / (_sampleImageWidth + 4 * _sampleImageMargin));
        }

        [ObservableProperty]
        int _span;

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
