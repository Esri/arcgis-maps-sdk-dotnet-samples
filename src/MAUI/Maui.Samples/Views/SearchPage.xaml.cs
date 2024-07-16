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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        SampleSearchBar.Focus();
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
}