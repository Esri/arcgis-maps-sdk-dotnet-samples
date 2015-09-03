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
using ArcGISRuntime.Desktop.Viewer.Managers;
using ArcGISRuntime.Samples.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArcGISRuntime.Samples.Desktop
{
    public partial class MainWindow
    {
        private bool _isTakingThumbnail;
        private int _previousHeight;
        private int _previousWidth;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            await SampleManager.Current.InitializeAsync(ApplicationManager.Current.SelectedLanguage);
 
            // Set featured data context
            featured.DataContext = SampleManager.Current.GetFeaturedSamples();
            featured.SelectedIndex = 0;

            // Set category data context
            categories.DataContext = SampleManager.Current.GetSamplesAsTree();
        }

        private void featured_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var featuredItem = e.AddedItems[0] as FeaturedModel;
                SelectSample(featuredItem.Sample);
            }
        }

        private void categoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var sample = e.AddedItems[0] as SampleModel;
                SelectSample(sample);
                (sender as ListView).SelectedItem = null;
            }
        }

        private void categories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sample = (e.NewValue as SampleModel);
            if (sample == null)
            {
                var treeItem = (e.NewValue as TreeItem);
                var samples = treeItem.Items.OfType<SampleModel>();
                if (samples.Count() > 0)
                {
                    categoriesList.ItemsSource = samples;
                    CategoriesRegion.Visibility = Visibility.Visible;
                }
            }
            else
                SelectSample(sample);
        }

        private void SelectSample(SampleModel selectedSample)
        {
            if (selectedSample == null) return;

            SampleManager.Current.SelectedSample = selectedSample;
            DescriptionContainer.DataContext = selectedSample;

            try
            {
                SampleContainer.Content = SampleManager.Current.SampleToControl(selectedSample);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception exception)
            {
                // failed to create new instance of the sample
                // TODO handle
            }
            CategoriesRegion.Visibility = Visibility.Collapsed;
        }

        private void Featured_Click(object sender, RoutedEventArgs e)
        {
            categories.Visibility = Visibility.Collapsed;
            featured.Visibility = Visibility.Visible;
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            featured.Visibility = Visibility.Collapsed;
            categories.Visibility = Visibility.Visible;
        }

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            TakeThumbnail();
        }

        private async void TakeThumbnail()
        {
            if (SampleManager.Current.SelectedSample == null)
            {
                MessageBox.Show("Please select Live Sample to before creating a thumbnail.");
                return;
            }

            if (!_isTakingThumbnail)
            {
                _previousHeight = (int)SampleContainer.ActualHeight;
                _previousWidth = (int)SampleContainer.ActualWidth;

                SampleContainer.Width = _previousHeight * 1.25;
                _isTakingThumbnail = true;
                return;
            }

            var rtb = new RenderTargetBitmap((int)SampleContainer.ActualWidth, 
                (int)SampleContainer.ActualHeight, 96, 96, PixelFormats.Default);
            rtb.Render(SampleContainer.Content as UIElement);

            // Encoding the RenderBitmapTarget as a JPG file.
            JpegBitmapEncoder jpg = new JpegBitmapEncoder() { QualityLevel = 90 };
            jpg.Frames.Add(BitmapFrame.Create(rtb));

            var file = new FileInfo(Path.Combine(
                SampleManager.Current.SelectedSample.GetSampleFolderInRelativeSolution(), 
                SampleManager.Current.SelectedSample.Image));
            if (file.Exists)
            {
                await Task.Delay(1000);
                file.Delete();
                using (Stream stm = File.Create(file.FullName))
                    jpg.Save(stm);
            }
            else
            {
                using (Stream stm = File.Create(file.FullName))
                    jpg.Save(stm);
            }

            SampleContainer.Width = _previousWidth;
            SampleContainer.Height = _previousHeight;
            SampleContainer.HorizontalAlignment = HorizontalAlignment.Left;
            SampleContainer.VerticalAlignment = VerticalAlignment.Top;
            _isTakingThumbnail = false;
        }

        private void LiveSample_Click(object sender, RoutedEventArgs e)
        {
            SampleContainer.Visibility = Visibility.Visible;
            DescriptionContainer.Visibility = Visibility.Collapsed;
        }

        private void Description_Click(object sender, RoutedEventArgs e)
        {
            SampleContainer.Visibility = Visibility.Collapsed;
            DescriptionContainer.Visibility = Visibility.Visible;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            closeNavigation.Visibility = Visibility.Collapsed;
            openNavigation.Visibility = Visibility.Visible;
            root.ColumnDefinitions[0].MaxWidth = 0;
        } 

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            openNavigation.Visibility = Visibility.Collapsed;
            closeNavigation.Visibility = Visibility.Visible;
            root.ColumnDefinitions[0].MaxWidth = 535;
        }
    }
}

