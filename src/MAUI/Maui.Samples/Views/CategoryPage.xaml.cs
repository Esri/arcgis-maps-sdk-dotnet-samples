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

        WeakReferenceMessenger.Default.Register<string>(this, (message, category) => ScrollToTop());

        // Handle platform differences in updating the layout.
#if !ANDROID
        // Event raised during an orientation state change or window resize. 
        SizeChanged += (s, e) =>
        {
            var numberOfColumns = (int)Math.Floor(Width / _viewModel.SampleImageWidth);
#if IOS || MACCATALYST
            // Don't update the layout when column count is the same, for example, when app height changes on Mac.
            if (numberOfColumns != (SamplesCollection.ItemsLayout as GridItemsLayout)?.Span)
            {
                SamplesCollection.ItemsLayout = new GridItemsLayout(numberOfColumns, ItemsLayoutOrientation.Vertical)
                {
                    HorizontalItemSpacing = 5,
                    VerticalItemSpacing = 5
                };
            }
#elif WINDOWS
            SamplesGridItemsLayout.Span = numberOfColumns;
#endif
        };
#else
        SamplesCollection.Style = Resources["AndroidStyle"] as Style;
#endif
    }

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

    // The favorites icon can flicker when using a pen as pointer.
    private void PointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
    {
#if WINDOWS || MACCATALYST
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (Button)grid.Children[1];

        imageButton.IsVisible = true;

        Console.WriteLine("PointerRecognized");
#endif
    }

    private void PointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
    {
 #if WINDOWS || MACCATALYST
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (Button)grid.Children[1];

        string sampleName = (string)imageButton.CommandParameter;

        imageButton.IsVisible = false || SampleManager.Current.IsSampleFavorited(sampleName);
#endif
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

    private void ScrollToTop()
    {
        var firstItem = _viewModel.SamplesItems.FirstOrDefault();
        if (firstItem != null)
        {
            SamplesCollection.ScrollTo(firstItem, null, ScrollToPosition.Start);
        }
    }
}