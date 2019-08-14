// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Color = System.Drawing.Color;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;
#if __IOS__
#endif
#if __ANDROID__

#endif
#if WINDOWS_UWP
using Windows.Storage.Pickers;
#endif

namespace ArcGISRuntimeXamarin.Samples.CreateAndSaveKmlFile
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create and save KML file",
        "Layers",
        "Construct a KML document and save it as a KMZ file.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class CreateAndSaveKmlFile : ContentPage
    {
        private KmlDocument _kmlDocument;
        private KmlDataset _kmlDataset;
        private KmlLayer _kmlLayer;
        private KmlPlacemark _currentPlacemark;

        public CreateAndSaveKmlFile()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map.
            MyMapView.Map = new Map(Basemap.CreateImagery());

            List<String> colorHexes = new List<string>()
            {
                "#000000","#FFFFFF","#FF0000","#00FF00","#0000FF","#FFFF00", "#00FFFF","#FF00FF", "#C0C0C0", "#808080","#800000", "#808000","#008000", "#800080","#008080","#000080"
            };
            ColorPicker.ItemsSource = colorHexes;

            // Set the images for the point icon picker.
            List<string> iconLinks = new List<string>()
            {
                "http://static.arcgis.com/images/Symbols/Shapes/BlueCircleLargeB.png",
                "http://static.arcgis.com/images/Symbols/Shapes/BlueDiamondLargeB.png",
                "http://static.arcgis.com/images/Symbols/Shapes/BluePin1LargeB.png",
                "http://static.arcgis.com/images/Symbols/Shapes/BluePin2LargeB.png",
                "http://static.arcgis.com/images/Symbols/Shapes/BlueSquareLargeB.png",
                "http://static.arcgis.com/images/Symbols/Shapes/BlueStarLargeB.png"
            };
            List<Uri> iconList = iconLinks.Select(x => new Uri(x)).ToList();
            IconPicker.ItemsSource = iconLinks;

            // Set up a new kml document and kml layer.
            ResetKml();
        }
        private void ResetKml()
        {
            // Clear any existing layers from the map.
            MyMapView.Map.OperationalLayers.Clear();

            // Reset the most recently placed placemark.
            _currentPlacemark = null;

            // Create a new KmlDocument.
            _kmlDocument = new KmlDocument() { Name = "KML Sample Document" };

            // Create a Kml dataset using the Kml document.
            _kmlDataset = new KmlDataset(_kmlDocument);

            // Create the kmlLayer using the kmlDataset.
            _kmlLayer = new KmlLayer(_kmlDataset);

            // Add the Kml layer to the map.
            MyMapView.Map.OperationalLayers.Add(_kmlLayer);
        }

        private async void Edit_Click(object sender, EventArgs e)
        {
            try
            {
                // Hide the base UI and enable the complete button.
                ShapeGrid.IsVisible = false;
                CompleteButton.IsVisible = true;
                SaveResetGrid.IsEnabled = false;

                // Create variables for the sketch creation mode and color.
                SketchCreationMode creationMode;

                // Set the creation mode and UI based on which button called this method.
                switch (((Button)sender).Text)
                {
                    case "Point":
                        creationMode = SketchCreationMode.Point;
                        Status.Text = "Tap to add a point.";
                        break;

                    case "Polyline":
                        creationMode = SketchCreationMode.Polyline;
                        Status.Text = "Tap to add a vertex.";
                        break;

                    case "Polygon":
                        creationMode = SketchCreationMode.Polygon;
                        Status.Text = "Tap to add a vertex.";
                        break;

                    default:
                        return;
                }

                // Get the user-drawn geometry.
                Geometry geometry = await MyMapView.SketchEditor.StartAsync(creationMode, true);

                // Project the geometry to WGS84 (WGS84 is required by the KML standard).
                Geometry projectedGeometry = GeometryEngine.Project(geometry, SpatialReferences.Wgs84);

                // Create a KmlGeometry using the new geometry.
                KmlGeometry kmlGeometry = new KmlGeometry(projectedGeometry, KmlAltitudeMode.ClampToGround);

                // Create a new placemark.
                _currentPlacemark = new KmlPlacemark(kmlGeometry);

                // Add the placemark to the KmlDocument.
                _kmlDocument.ChildNodes.Add(_currentPlacemark);

                // Choose whether to enable the icon picker or color picker.
                IconPicker.IsVisible = creationMode == SketchCreationMode.Point;
                ColorPicker.IsVisible = creationMode != SketchCreationMode.Point;

                // Enable the style editing UI.
                StyleUI.IsVisible = true;
                MainUI.IsVisible = false;
            }
            finally
            {
                // Reset the UI.
                ShapeGrid.IsVisible = true;
                CompleteButton.IsVisible = false;
                Status.Text = "Select the type of feature you would like to add.";

                // Enable the save and reset buttons.
                SaveResetGrid.IsEnabled = true;
            }
        }

        private void Apply_Style_Click(object sender, SelectedItemChangedEventArgs e)
        {
            // Get the color value if the selected item is a hexadecimal color.
            Color color = Color.Transparent;
            if (((string)e.SelectedItem).StartsWith('#'))
            {
                var platColor = Xamarin.Forms.Color.FromHex((string)e.SelectedItem);
                color = Color.FromArgb((int)(platColor.A * 255), (int)(platColor.R * 255), (int)(platColor.G * 255), (int)(platColor.B * 255));
            }

            // Create a new style for the placemark.
            _currentPlacemark.Style = new KmlStyle();

            // Set the style for that Placemark.
            switch (_currentPlacemark.Geometries.FirstOrDefault().Type)
            {
                // Create a KmlIconStyle using the selected icon.
                case KmlGeometryType.Point:
                    Uri iconLink = new Uri((string)e.SelectedItem);
                    _currentPlacemark.Style.IconStyle = new KmlIconStyle(new KmlIcon(iconLink), 1.0);
                    break;

                // Create a KmlLineStyle using the selected color value.
                case KmlGeometryType.Polyline:
                    _currentPlacemark.Style.LineStyle = new KmlLineStyle(color, 8);
                    break;

                // Create a KmlPolygonStyle using the selected color value.
                case KmlGeometryType.Polygon:
                    _currentPlacemark.Style.PolygonStyle = new KmlPolygonStyle(color);
                    _currentPlacemark.Style.PolygonStyle.IsFilled = true;
                    _currentPlacemark.Style.PolygonStyle.IsOutlined = false;
                    break;
            }

            // Reset the UI.
            MainUI.IsVisible = true;
            StyleUI.IsVisible = false;
        }

        private void No_Style_Click(object sender, EventArgs e)
        {
            // Reset the UI.
            StyleUI.IsVisible = false;
            MainUI.IsVisible = true;
        }

        private void Complete_Click(object sender, EventArgs e)
        {
            try
            {
                // Finish the sketch.
                MyMapView.SketchEditor.CompleteCommand.Execute(null);
            }
            catch (ArgumentException)
            {
            }
        }

        private async void Save_Click(object sender, EventArgs e)
        {
            // oof
#if __IOS__
            try
            {
                string offlineDataFolder = Path.Combine(DataManager.GetDataFolder(), "CreateAndSaveKmlFile");

                // If temporary data folder doesn't exists, create it.
                if (!Directory.Exists(offlineDataFolder))
                {
                    Directory.CreateDirectory(offlineDataFolder);
                }

                string path = Path.Combine(offlineDataFolder, "sampledata.kmz");
                using (Stream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    // Write the KML document to the stream of the file.
                    await _kmlDocument.WriteToAsync(stream);
                }
                await Application.Current.MainPage.DisplayAlert("Success", "KMZ file saved locally to ArcGISRuntimeSamples folder.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "File not saved.", "OK");
            }

#endif
#if __ANDROID__
            try
            {
                string offlineDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal));

                // If temporary data folder doesn't exists, create it.
                if (!Directory.Exists(offlineDataFolder))
                {
                    Directory.CreateDirectory(offlineDataFolder);
                }

                string path = Path.Combine(offlineDataFolder, "sampledata.kmz");
                using (Stream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    // Write the KML document to the stream of the file.
                    await _kmlDocument.WriteToAsync(stream);
                }
                await Application.Current.MainPage.DisplayAlert("Success", "KMZ file saved locally to ArcGISRuntimeSamples folder.", "OK");
            }
            catch(Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "File not saved.", "OK");
            }

#endif
#if WINDOWS_UWP
            // Open a save dialog for the user.
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("KMZ file", new List<string>() { ".kmz" });
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                using (Stream stream = await file.OpenStreamForWriteAsync())
                {
                    // Write the KML document to the stream of the file.
                    await _kmlDocument.WriteToAsync(stream);
                }
            }
#endif
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            ResetKml();
        }
    }
    internal class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(string)) { return null; }
            return ImageSource.FromUri(new Uri((string)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(string)) { return null; }
            return Xamarin.Forms.Color.FromHex((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}