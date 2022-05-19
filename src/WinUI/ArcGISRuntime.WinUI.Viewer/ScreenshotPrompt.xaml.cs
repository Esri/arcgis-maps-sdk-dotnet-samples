// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISRuntime
{
    public sealed partial class ScreenshotPrompt : UserControl
    {
        public ScreenshotPrompt()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            ScreenshotEnabledCheckBox.IsChecked = ScreenshotManager.ScreenshotSettings.ScreenshotEnabled;
            SourcePathText.Text = ScreenshotManager.ScreenshotSettings.SourcePath;
            WidthEntryBox.Text = ScreenshotManager.ScreenshotSettings.Width.HasValue ? ScreenshotManager.ScreenshotSettings.Width.ToString() : null;
            HeightEntryBox.Text = ScreenshotManager.ScreenshotSettings.Height.HasValue ? ScreenshotManager.ScreenshotSettings.Height.ToString() : null;
            ScaleFactorEntryBox.Text = ScreenshotManager.ScreenshotSettings.ScaleFactor.HasValue ? ScreenshotManager.ScreenshotSettings.ScaleFactor.ToString() : null;
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

            if (double.TryParse(ScaleFactorEntryBox.Text, out double scaleFactor))
            {
                screenshotSettings.ScaleFactor = scaleFactor;
            }
            else
            {
                screenshotSettings.ScaleFactor = null;
            }

            ScreenshotManager.SaveScreenshotSettings(screenshotSettings);

            Status.Text = "Saved";
        }

        private void IntegerInput_BeforeTextChanging(object sender, TextBoxBeforeTextChangingEventArgs e)
        {
            e.Cancel = !IsIntegerTextAllowed(e.NewText);
        }

        private bool IsIntegerTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        private void DoubleInput_BeforeTextChanging(object sender, TextBoxBeforeTextChangingEventArgs e)
        {
            e.Cancel = !IsDoubleTextAllowed(e.NewText);
        }

        private bool IsDoubleTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.]+");
            return !regex.IsMatch(text);
        }
    }
}
