using ArcGIS.Helpers;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;
using ArcGIS.ViewModels;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;

namespace ArcGIS;

public partial class CategoryPage : ContentPage
{
    private CategoryViewModel _viewModel;

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

        WeakReferenceMessenger.Default.Register<string>(this, async (message, category) => await ScrollToTop());
    }

#if ANDROID || IOS
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        var numberOfColumns = Math.Floor(width / (_viewModel.SampleImageWidth + 4 * _viewModel.SampleImageMargin));

        if (numberOfColumns == 0) return;

        if (numberOfColumns > 1)
        {
            SamplesCollection.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start;
            SamplesCollection.HorizontalOptions = LayoutOptions.Fill;
            SamplesScrollView.HorizontalOptions = LayoutOptions.Fill;
        }
        else
        {
            SamplesCollection.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Center;
            SamplesCollection.HorizontalOptions = LayoutOptions.Center;
            SamplesScrollView.HorizontalOptions = LayoutOptions.Center;
        }
    }
#endif

    private async void FeedbackToolbarItem_Clicked(object sender, EventArgs e)
    {
        await FeedbackPrompt.ShowFeedbackPromptAsync();
    }

    private async void SettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage(), true);
    }

    private async void SearchClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SearchPage(), false);
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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Ensure we enable the API key when we navigate here at any time. 
        if (ApiKeyManager.KeyDisabled)
        {
            ApiKeyManager.EnableKey();
        }
    }

    private async Task ScrollToTop()
    {
        var firstItem = _viewModel.SamplesItems.FirstOrDefault();
        if (firstItem != null)
        {
            await SamplesScrollView.ScrollToAsync(0, 0, false);
        }
    }
}