using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Models;
using ArcGIS.ViewModels;

namespace ArcGIS;

public partial class SearchPage : ContentPage
{
    private SearchViewModel _viewModel;

	public SearchPage()
	{
		InitializeComponent();
        _viewModel = new SearchViewModel();
        BindingContext = _viewModel;
	}

    private void TapGestureRecognizer_SearchResultTapped(object sender, TappedEventArgs e)
    {
        var sampleInfo = e.Parameter as SampleInfo;
        _ = SampleLoader.LoadSample(sampleInfo);
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(false);
    }

    private async void FieldToolbarItem_Clicked(object sender, EventArgs e)
    {
        var result = await DisplayActionSheet("Select a field", "Cancel", null, "Sample Name", "Sample Category");

        if (!string.IsNullOrEmpty(result) && result != "Cancel")
        {
            _viewModel.SearchField = result;
        }
    }

    private async void OrderToolbarItem_Clicked(object sender, EventArgs e)
    {
        string result = await DisplayActionSheet("Order direction", "Cancel", null, "Ascending", "Descending");

        if (!string.IsNullOrEmpty(result) && result != "Cancel")
        {
            _viewModel.SearchOrder = result;
        }
    }
}