using ArcGISRuntime.Samples.Shared.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISRuntime
{
    /// <summary>
    /// Interaction logic for ScreenshotPrompt.xaml
    /// </summary>
    public partial class ScreenshotPrompt : UserControl
    {
        public ScreenshotPrompt()
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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
            
            Status.Text = "Saved";
        }

        private void IntegerInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }
    }
}
