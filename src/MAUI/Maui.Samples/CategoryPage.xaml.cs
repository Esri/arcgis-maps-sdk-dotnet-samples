using ArcGIS.Helpers;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using ArcGIS.ViewModels;
using CommunityToolkit.Maui.Views;

namespace ArcGIS;

public partial class CategoryPage : ContentPage
{
    private SearchableTreeNode _category;

    public CategoryPage(SearchableTreeNode category)
    {
        _category = category;

        InitializeComponent();

        Initialize();
    }

    private void Initialize()
    {
        SetBindingContext();

        Title = _category.Name;
    }

    private void SetBindingContext()
    {
        // Get the samples from the category.
        var listSampleItems = _category?.Items.OfType<SampleInfo>().ToList();

        // Update the binding to show the samples.
        BindingContext = new CategoryViewModel(listSampleItems, _category.Name);
    }

    private async void SettingsClicked(object sender, EventArgs e)
    {
        await this.Navigation.PushAsync(new SettingsPage(), true);
    }

    private async void SearchClicked(object sender, EventArgs e)
    {
        var popup = new SearchPopup();

        var result = await this.ShowPopupAsync(popup);
    }

    private void TapGestureRecognizer_SampleTapped(object sender, TappedEventArgs e)
    {
        var sampleInfo = e.Parameter as SampleInfo;
        _ = SampleLoader.LoadSample(sampleInfo);
    }

    private void PointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
    {
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (Button)grid.Children[1];

        imageButton.IsVisible = true;

        Console.WriteLine("PointerRecognized");
    }

    private void PointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
    {
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (Button)grid.Children[1];

        string sampleName = (string)imageButton.CommandParameter;

        imageButton.IsVisible = false || SampleManager.Current.IsSampleFavorited(sampleName);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ensure the favorite category is up to date with the correct favorited samples. 
        if (_category.Name == "Favorites")
        {
            _category = SampleManager.Current.GetFavoritesCategory();

            SetBindingContext();
        }
    }
}