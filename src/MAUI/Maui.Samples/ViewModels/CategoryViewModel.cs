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

            double sampleImageWidth = 400;

#if WINDOWS

            sampleImageWidth  = 300;

#elif IOS || ANDROID

            var displayWidth = DeviceDisplay.MainDisplayInfo.Width;
            var displayDensity = DeviceDisplay.MainDisplayInfo.Density;

            sampleImageWidth = displayWidth / displayDensity - 10;
#endif

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
        string _sampleImageName;

        [ObservableProperty]
        SampleInfo _sampleObject;

        [ObservableProperty]
        double _sampleImageWidth;

        [ObservableProperty]
        double _sampleImageHeight;

    }
}
