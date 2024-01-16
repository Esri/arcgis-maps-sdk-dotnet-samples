using ArcGIS.Helpers;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;
using ArcGIS.Samples.Shared.Models;
using ArcGIS.ViewModels;
using CommunityToolkit.Maui.Views;

namespace ArcGIS;

[QueryProperty(nameof(CategoryName), "CategoryName")]
public partial class CategoryPage : ContentPage
{
    private CategoryViewModel _viewModel;

    string _categoryName;
    public string CategoryName 
    { 
        get => _categoryName; 
        set 
        { 
            _categoryName = value;
            _viewModel.UpdateCategory(value);
        } 
    }

    public CategoryPage()
    {
        InitializeComponent();

        Initialize();
    }

    private void Initialize()
    {
        // Update the binding to show the samples.
        _viewModel = new CategoryViewModel();
        BindingContext = _viewModel;
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
        //base.OnAppearing();

        //// Ensure the favorite category is up to date with the correct favorited samples. 
        //if (_category.Name == "Favorites")
        //{
        //    _category = SampleManager.Current.GetFavoritesCategory();
        //}
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Ensure we enable the API key when we navigate here at any time. 
        if (ApiKeyManager.KeyDisabled)
        {
            ApiKeyManager.EnableKey();
        }
    }
}