using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;

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
    }

    #region Check API Key
    private void FirstLoaded(object sender, EventArgs e)
    {
        this.Appearing -= FirstLoaded;

        SampleManager.Current.Initialize();

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