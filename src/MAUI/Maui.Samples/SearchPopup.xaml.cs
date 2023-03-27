using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Maui.Views;

namespace ArcGIS;

public partial class SearchPopup : Popup
{
    private NavigableElement _categoryPage;

	public SearchPopup(NavigableElement categoryPage)
	{
		InitializeComponent();

        _categoryPage = categoryPage;
	}

    private void TapGestureRecognizer_SearchResultTapped(object sender, TappedEventArgs e)
    {
        this.Close();

        var sampleInfo = e.Parameter as SampleInfo;
        _ = SampleLoader.LoadSample(sampleInfo, _categoryPage);
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
		this.Close();
    }
}