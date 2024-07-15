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
    }

#if WINDOWS || MACCATALYST
    
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        _viewModel.UpdateGridSpan(width);
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

    private void ScrollToTop()
    {
        var firstItem = _viewModel.SamplesItems.FirstOrDefault();
        if (firstItem != null)
        {
            SamplesCollection.ScrollTo(0);
        }
    }
}