using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArcGIS.ViewModels
{
    public partial class SearchResultViewModel : ObservableObject
    {
        public SearchResultViewModel(SampleInfo sampleResult, int score)
        {
            SampleName = sampleResult.SampleName;
            SampleCategory = sampleResult.Category;
            SampleDescription = sampleResult.Description;
            SampleImage = new FileImageSource() { File = sampleResult.SampleImageName };
            Score = score;
            SampleObject = sampleResult;
        }

        [ObservableProperty]
        int _score;

        [ObservableProperty]
        string _sampleName;

        [ObservableProperty]
        string _sampleCategory;

        [ObservableProperty]
        string _sampleDescription;

        [ObservableProperty]
        ImageSource _sampleImage;

        [ObservableProperty]
        SampleInfo _sampleObject;
    }
}
