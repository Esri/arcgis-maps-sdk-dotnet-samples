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

        //FlyoutItem flyoutItem = new FlyoutItem();
        //flyoutItem.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;

        //Items.Add(flyoutItem);

        foreach (var category in samplesCategories)
        {
            ShellContent shellContent = new ShellContent();
            shellContent.Title = category.Name;
            shellContent.Content = new CategoryPage(category);
            shellContent.Route = $"{nameof(CategoryPage)}_{category.Name}";

            CategoriesTabBar.Items.Add(shellContent);

            //Tab tab = new Tab();
            //tab.Title = category.Name;
            //flyoutItem.Items.Add(tab);

            //foreach (SampleInfo sample in category.Items)
            //{
            //    ShellContent shellContent = new ShellContent();
            //    shellContent.Title = sample.SampleName;

            //    ContentPage sampleControl = (ContentPage)SampleManager.Current.SampleToControl(sample);

            //    shellContent.Content = new SamplePage(sampleControl, sample);
            //    shellContent.Route = $"{nameof(SamplePage)}_{sample.FormalName}";

            //    tab.Items.Add(shellContent);
            //}
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
            await Navigation.PushAsync(new ApiKeyPrompt(), true);
        }
    }
    #endregion
}