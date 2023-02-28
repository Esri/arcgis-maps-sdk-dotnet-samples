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

	public CategoryPage(SearchableTreeNode category)
	{
		_category = category;

        InitializeComponent();

        Initialize();
	}

    private void Initialize()
    {
        // Get the samples from the category.
        var listSampleItems = _category?.Items.OfType<SampleInfo>().ToList();

        // Update the binding to show the samples.
        BindingContext = listSampleItems;
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

        var childGrid = (Grid)grid.Children[1];

        var imageButton = (ImageButton)childGrid[0];

        imageButton.IsVisible = true;// = Colors.Pink;

        Console.WriteLine("PointerRecognized");
    }

    private void PointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
    {
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var childGrid = (Grid)grid.Children[1];

        var imageButton = (ImageButton)childGrid[0];

        imageButton.IsVisible = false; // App.Current.PlatformAppTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? Colors.White : Colors.Black;

        Console.WriteLine("PointerRecognized");
    }

    private void FavoriteButton_Clicked(object sender, EventArgs e)
    {
        var favoriteButton = sender as ImageButton;

        string sampleFormalName = favoriteButton.CommandParameter.ToString();

        SampleManager.Current.AddRemoveFavorite(sampleFormalName);

        //// Fill or empty the star icon.
        //favoriteButton.Source = (ImageSource)new BoolToImageSourceConverter().Convert(SampleManager.Current.IsFavorite(sampleFormalName), Type.GetType("ImageSource"), null, null);
    }
}