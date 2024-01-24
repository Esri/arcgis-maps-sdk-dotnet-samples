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
        private static readonly string DefaultCategory = "Featured";
        private double _sampleImageWidth;
        private double _sampleImageHeight;
        public double SampleImageWidth => _sampleImageWidth;

        public CategoryViewModel()
        {
            // Calculate the sample image width and height on mobile platforms based on device display size. 
            _sampleImageWidth = 400;

#if WINDOWS

            _sampleImageWidth  = 300;

#elif IOS || ANDROID

            // Ensure that the correct dimension is being used, this accounts for situations where the viewer is opened in landscape orientation. 
            var displayWidth = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Width : DeviceDisplay.MainDisplayInfo.Height;

            var displayDensity = DeviceDisplay.MainDisplayInfo.Density;

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
                SamplesItems.Add(new SampleViewModel(sampleInfo, _sampleImageWidth, _sampleImageHeight));
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
            var samplesCollection = samples.Select(s => new SampleViewModel(s, _sampleImageWidth, _sampleImageHeight)).ToObservableCollection();
            SamplesItems = samplesCollection;
        }

        private List<SampleInfo> GetSamplesInCategory(string category)
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
        void SampleSelected(SampleViewModel sampleViewModel)
        {
            _ = SampleLoader.LoadSample(sampleViewModel.SampleObject);
        }
    }

    public partial class SampleViewModel : ObservableObject
    {
        public SampleViewModel(SampleInfo sampleInfo, double sampleImageWidth, double sampleImageHeight)
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
    }
}
