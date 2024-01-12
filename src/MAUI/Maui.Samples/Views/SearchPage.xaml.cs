using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Models;

namespace ArcGIS;

public partial class SearchPage : ContentPage
{
	public SearchPage()
	{
		InitializeComponent();
	}

    private void TapGestureRecognizer_SearchResultTapped(object sender, TappedEventArgs e)
    {
        var sampleInfo = e.Parameter as SampleInfo;
        _ = SampleLoader.LoadSample(sampleInfo);

        this.Navigation.PopModalAsync(false);
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(false);
    }
}