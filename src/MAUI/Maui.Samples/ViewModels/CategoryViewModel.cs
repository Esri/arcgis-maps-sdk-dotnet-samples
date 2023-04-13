using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcGIS.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private string _category;

        public CategoryViewModel(List<SampleInfo> sampleInfos, string category)
        {
            _samplesItems = new ObservableCollection<SampleViewModel>();

            // Calculate the sample image width and height on mobile platforms based on device display size. 
            double sampleImageWidth = 400;

#if WINDOWS

            sampleImageWidth  = 300;

#elif IOS || ANDROID

            // Ensure that the correct dimension is being used, this accounts for situations where the viewer is opened in landscape orientation. 
            var displayWidth = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Width : DeviceDisplay.MainDisplayInfo.Height;

            var displayDensity = DeviceDisplay.MainDisplayInfo.Density;

            // Calculate the width for the image using the device display density. Account for a margin around the image. 
            sampleImageWidth = displayWidth / displayDensity - 20;
            
            // For tablets check to see if multiple images could fit rather than one tablet sized image. 
            // If multiple images of arbitrary size "400" would fit then update the image width.
            var sampleImageFactor = Math.Floor(sampleImageWidth / 300);

            if (sampleImageFactor > 1)
            {
                sampleImageWidth = sampleImageWidth / sampleImageFactor;
            }
#endif
            // Maintain 4:3 image resolution. 
            double sampleImageHeight = Math.Floor(sampleImageWidth * 3 / 4);

            foreach (var sampleInfo in sampleInfos)
            {
                SamplesItems.Add(new SampleViewModel(sampleInfo, sampleImageWidth, sampleImageHeight));
            }

            _category = category;
        }

        [ObservableProperty]
        ObservableCollection<SampleViewModel> _samplesItems;

        [RelayCommand]
        void UpdateFavorite(string sampleFormalName)
        {
            var sample = SamplesItems.FirstOrDefault(sample => sample.SampleFormalName == sampleFormalName);

            sample.IsFavorite = !sample.IsFavorite;

            SampleManager.Current.AddRemoveFavorite(sampleFormalName);

            if (_category == "Favorites" && SamplesItems.Contains(sample))
            {
                SamplesItems.Remove(sample);
            }

            
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

        [ObservableProperty]
        SampleInfo _sampleObject;

        [ObservableProperty]
        double _sampleImageWidth;

        [ObservableProperty]
        double _sampleImageHeight;

    }
}
