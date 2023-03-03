using ArcGIS.Helpers;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Maui;
using ArcGIS.Samples.Shared.Models;
using Microsoft.Maui.Graphics;
using System.Drawing;

namespace ArcGIS;

public partial class CategoryPage : ContentPage
{
	private SearchableTreeNode _category;
    private List<SampleInfo> _listSampleItems;
    private bool IsOnMobile = true;

	public CategoryPage(SearchableTreeNode category)
	{
		_category = category;

        InitializeComponent();

        Initialize();
	}

    private void Initialize()
    {
        SetBindingContext();
    }

    private void SetBindingContext()
    {
        // Get the samples from the category.
        _listSampleItems = _category?.Items.OfType<SampleInfo>().ToList();

        // Update the binding to show the samples.
        BindingContext = _listSampleItems;
    }

    private async void SettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new SettingsPage(), true);
    }


    private void TapGestureRecognizer_SampleTapped(object sender, TappedEventArgs e)
    {
        var sampleInfo = e.Parameter as SampleInfo;
        _ = SampleLoader.LoadSample(sampleInfo, this);
    }

    private void PointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
    {
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (ImageButton)grid.Children[1];

        imageButton.IsVisible = true;// = Colors.Pink;

        Console.WriteLine("PointerRecognized");
    }

    private void PointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
    {
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (ImageButton)grid.Children[1];

        SampleInfo pointerSample = imageButton.CommandParameter as SampleInfo;

        imageButton.IsVisible = false || SampleManager.Current.IsSampleFavorited(pointerSample.FormalName); 
    }

    private void FavoriteButton_Clicked(object sender, EventArgs e)
    {
        var favoriteButton = sender as ImageButton;

        SampleInfo clickedSample = favoriteButton.CommandParameter as SampleInfo;

        SampleManager.Current.AddRemoveFavorite(clickedSample.FormalName);

        // Get the samples from the category.
        var listSampleItems = _category?.Items.OfType<SampleInfo>().ToList();

        if(_category.Name == "Favorites" && !clickedSample.IsFavorite)
        {
            _category = SampleManager.Current.GetFavoritesCategory();

            listSampleItems.Remove(clickedSample);
        }

        // Update the binding to show the newly favorited samples.
        BindingContext = listSampleItems;
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