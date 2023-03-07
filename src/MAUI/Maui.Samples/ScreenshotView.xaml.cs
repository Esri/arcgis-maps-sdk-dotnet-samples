using ArcGIS.Samples.Shared.Managers;
using ArcGIS.Samples.Shared.Models;

namespace ArcGIS;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ScreenshotView : ContentView
{
    public ScreenshotView()
    {
        InitializeComponent();
        Initialize();
    }

    private void Initialize()
    {
        ScreenshotEnabledCheckBox.IsChecked = ScreenshotManager.ScreenshotSettings.ScreenshotEnabled;
        SourcePathEntry.Text = ScreenshotManager.ScreenshotSettings.SourcePath;
        WidthEntry.Text = ScreenshotManager.ScreenshotSettings.Width.HasValue ? ScreenshotManager.ScreenshotSettings.Width.ToString() : null;
        HeightEntry.Text = ScreenshotManager.ScreenshotSettings.Height.HasValue ? ScreenshotManager.ScreenshotSettings.Height.ToString() : null;
        ScaleFactorEntry.Text = ScreenshotManager.ScreenshotSettings.ScaleFactor.HasValue ? ScreenshotManager.ScreenshotSettings.ScaleFactor.ToString() : null;
    }

    private void SaveData()
    {
        ScreenshotSettings screenshotSettings = new ScreenshotSettings();

        // Do not overwrite the saved WinUI setting.
        if (ScreenshotManager.ScreenshotSettings.ScaleFactor.HasValue)
        {
            screenshotSettings.ScaleFactor = ScreenshotManager.ScreenshotSettings.ScaleFactor.Value;
        }

        screenshotSettings.ScreenshotEnabled = ScreenshotEnabledCheckBox.IsChecked ? ScreenshotEnabledCheckBox.IsChecked : false;
        screenshotSettings.SourcePath = SourcePathEntry.Text;

        if (int.TryParse(WidthEntry.Text, out int width))
        {
            screenshotSettings.Width = width;
        }
        else
        {
            screenshotSettings.Height = null;
        }

        if (int.TryParse(HeightEntry.Text, out int height))
        {
            screenshotSettings.Height = height;
        }
        else
        {
            screenshotSettings.Height = null;
        }

        ScreenshotManager.SaveScreenshotSettings(screenshotSettings);
    }

    private void ScreenshotEnabledCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        SaveData();
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveData();
    }
}