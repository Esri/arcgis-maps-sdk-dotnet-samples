//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using ArcGISRuntime.Samples.Managers;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Windows.Viewer
{
    public sealed partial class SamplePage 
    {
        public SamplePage()
        {
            InitializeComponent();

            // Get selected sample and set that as a DataContext
            DataContext = SampleManager.Current.SelectedSample;
            // Set loaded sample to the UI 
            SampleContainer.Content = SampleManager.Current.SampleToControl(SampleManager.Current.SelectedSample);
            LiveSample.IsChecked = true; // Default to the live sample view
        }

        private void LiveSample_Checked(object sender, RoutedEventArgs e)
        {
            // Make sure that only one is  selected
            if (Description.IsChecked.HasValue && Description.IsChecked.Value)
                Description.IsChecked = false;

            DescriptionContainer.Visibility = Visibility.Collapsed;
            SampleContainer.Visibility = Visibility.Visible;
        }

        private void Description_Checked(object sender, RoutedEventArgs e)
        {
            // Make sure that only one is  selected
            if (LiveSample.IsChecked.HasValue && LiveSample.IsChecked.Value)
                LiveSample.IsChecked = false;

            DescriptionContainer.Visibility = Visibility.Visible;
            SampleContainer.Visibility = Visibility.Collapsed;
        }
    }
}
