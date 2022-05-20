using ArcGISRuntime.Samples.Shared.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntime
{
    /// <summary>
    /// Interaction logic for ScreenshotPrompt.xaml
    /// </summary>
    public partial class ScreenshotTab : UserControl
    {
        public ScreenshotTab()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            ScreenshotEnabledCheckBox.IsChecked = ScreenshotManager.ScreenshotSettings.ScreenshotEnabled;
            SourcePathText.Text = ScreenshotManager.ScreenshotSettings.SourcePath;
            WidthEntryBox.Text = ScreenshotManager.ScreenshotSettings.Width.HasValue ? ScreenshotManager.ScreenshotSettings.Width.ToString() : null;
            HeightEntryBox.Text = ScreenshotManager.ScreenshotSettings.Height.HasValue ? ScreenshotManager.ScreenshotSettings.Height.ToString() : null;

            ScreenshotEnabledCheckBox.Checked += SaveData_Event;
            SourcePathText.TextChanged += SaveData_Event;
            WidthEntryBox.TextChanged += SaveData_Event;
            HeightEntryBox.TextChanged += SaveData_Event;
        }

        private void SaveData_Event(object sender, RoutedEventArgs e)
        {
            ScreenshotSettings screenshotSettings = new ScreenshotSettings();
            screenshotSettings.ScreenshotEnabled = ScreenshotEnabledCheckBox.IsChecked.HasValue ? ScreenshotEnabledCheckBox.IsChecked.Value : false;
            screenshotSettings.SourcePath = SourcePathText.Text;

            if (int.TryParse(WidthEntryBox.Text, out int width))
            {
                screenshotSettings.Width = width;
            }
            else
            {
                screenshotSettings.Height = null;
            }

            if (int.TryParse(HeightEntryBox.Text, out int height))
            {
                screenshotSettings.Height = height;
            }
            else
            {
                screenshotSettings.Height = null;
            }

            ScreenshotManager.SaveScreenshotSettings(screenshotSettings);
        }
    }
}
