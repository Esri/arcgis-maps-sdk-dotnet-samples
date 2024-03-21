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

    private const string BugReport = "Bug Report";
    private const string FeatureRequest = "Feature Request";

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
#if !ANDROID
        SizeChanged += (s, e) =>
        {
#if IOS || MACCATALYST
            var numberOfColumns = Math.Floor(Width / _viewModel.SampleImageWidth);
            var layout = new GridItemsLayout((int)numberOfColumns, ItemsLayoutOrientation.Vertical);
            layout.HorizontalItemSpacing = 5;
            layout.VerticalItemSpacing = 5;
            SamplesCollection.ItemsLayout = layout;
#elif WINDOWS
            var numberOfColumns = Math.Floor(Width / _viewModel.SampleImageWidth);
            SamplesGridItemsLayout.Span = (int)numberOfColumns;
#endif
        };
#endif

    }

    private async void FeedbackToolbarItem_Clicked(object sender, EventArgs e)
    {
        await DisplayActionSheet("Open an issue on GitHub:", "Cancel", null, [BugReport, FeatureRequest]).ContinueWith((result) =>
        {
            string link;
            switch (result.Result)
            {
                case BugReport:
                    link =
                        "https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=Type%3A+Bug&projects=&template=bug_report.yml&title=%5BBug%5D";
                    _ = Browser.Default.OpenAsync(new Uri(link), BrowserLaunchMode.SystemPreferred);
                    break;

                case FeatureRequest:
                    link =
                        "https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=Type%3A+Feature&projects=&template=feature_request.yml&title=%5BFeature%5D";
                    _ = Browser.Default.OpenAsync(new Uri(link), BrowserLaunchMode.SystemPreferred);
                    break;

                case "Cancel":
                    break;
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
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

    private void ScrollToTop()
    {
        var firstItem = _viewModel.SamplesItems.FirstOrDefault();
        if (firstItem != null)
        {
            SamplesCollection.ScrollTo(firstItem, null, ScrollToPosition.Start);
        }
    }
}