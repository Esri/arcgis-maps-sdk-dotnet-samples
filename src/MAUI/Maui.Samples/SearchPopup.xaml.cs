using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Maui.Views;

namespace ArcGIS;

public partial class SearchPopup : Popup
{
	public SearchPopup()
	{
		InitializeComponent();
	}

    private void TapGestureRecognizer_SearchResultTapped(object sender, TappedEventArgs e)
    {
        this.Close();

        var sampleInfo = e.Parameter as SampleInfo;
        _ = SampleLoader.LoadSample(sampleInfo);
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
		this.Close();
    }
}