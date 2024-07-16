using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Managers;
using ArcGIS.ViewModels;

namespace ArcGIS;

public partial class CategoryPage : ContentPage
{
    public CategoryPage(CategoryViewModel categoryViewModel)
    {
        InitializeComponent();

        BindingContext = categoryViewModel;
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