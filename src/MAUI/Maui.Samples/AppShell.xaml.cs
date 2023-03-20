using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;
using ArcGIS.Samples.Shared.Models;

namespace ArcGIS;

public partial class AppShell : Shell
{
	public AppShell()
	{
        InitializeComponent();

        Initialize();
	}

    private void Initialize()
    {
        this.Appearing += FirstLoaded;

        SampleManager.Current.Initialize();

        var samplesCategories = SampleManager.Current.FullTree.Items.OfType<SearchableTreeNode>().ToList();
        var allSamples = SampleManager.Current.AllSamples.ToList();

        List<FlyoutItem> flyoutItems = new List<FlyoutItem>();

        foreach (var category in samplesCategories)
        {
            FlyoutItem flyoutItem = new FlyoutItem();
            flyoutItem.Title = category.Name;

            ShellContent shellContent = new ShellContent();
            //shellContent.Title = category.Name;
            shellContent.Content = new CategoryPage(category);
            shellContent.Route = $"{nameof(CategoryPage)}_{category.Name}";

            flyoutItem.Items.Add(shellContent);

            this.Items.Add(flyoutItem);
        }
    }

    #region Check API Key
    private void FirstLoaded(object sender, EventArgs e)
    {
        this.Appearing -= FirstLoaded;

        _ = CheckApiKey();
    }

    private async Task CheckApiKey()
    {
        // Attempt to load a locally stored API key.
        await ApiKeyManager.TrySetLocalKey();

        // Check that the current API key is valid.
        ApiKeyStatus status = await ApiKeyManager.CheckKeyValidity();
        if (status != ApiKeyStatus.Valid)
        {
            await Navigation.PushAsync(new ApiKeyPage(), true);
        }
    }
    #endregion
}