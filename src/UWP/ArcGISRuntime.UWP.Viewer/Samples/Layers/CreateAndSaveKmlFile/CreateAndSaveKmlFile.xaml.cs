// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;

namespace ArcGISMapsSDK.UWP.Samples.CreateAndSaveKmlFile
{
    [ArcGISMapsSDK.Samples.Shared.Attributes.Sample(
        name: "Create and save KML file",
        category: "Layers",
        description: "Construct a KML document and save it as a KMZ file.",
        instructions: "Click on one of the buttons in the middle row to start adding a geometry. Click on the map view to place vertices. Click the \"Complete Sketch\" button to add the geometry to the KML document as a new KML placemark. Use the style interface to edit the style of the placemark. If you do not wish to set a style, click the \"Don't Apply Style\" button. When you are finished adding KML nodes, click on the \"Save KMZ file\" button to save the active KML document as a .kmz file on your system. Use the \"Reset\" button to clear the current KML document and start a new one.",
        tags: new[] { "KML", "KMZ", "Keyhole", "OGC" })]
    [ArcGISMapsSDK.Samples.Shared.Attributes.OfflineData()]
    public partial class CreateAndSaveKmlFile
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
            MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

            // Set the images for the point icon picker.
            List<string> iconLinks = new List<string>()
            {
                "https://static.arcgis.com/images/Symbols/Shapes/BlueCircleLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueDiamondLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BluePin1LargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BluePin2LargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueSquareLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueStarLargeB.png"
            };
            List<Uri> _iconList = iconLinks.Select(x => new Uri(x)).ToList();
            IconPicker.ItemsSource = _iconList;
            IconPicker.SelectedIndex = 0;

            // Set up a new kml document and kml layer.
            ResetKml();
        }

        private void ResetKml()
        {
            // Clear any existing layers from the map.
            MyMapView.Map.OperationalLayers.Clear();

            // Reset the most recently placed placemark.
            _currentPlacemark = null;

            // Create a new KML document.
            _kmlDocument = new KmlDocument() { Name = "KML Sample Document" };

            // Create a KML dataset using the KML document.
            _kmlDataset = new KmlDataset(_kmlDocument);

            // Create the KML layer using the KML dataset.
            _kmlLayer = new KmlLayer(_kmlDataset);

            // Add the KML layer to the map.
            MyMapView.Map.OperationalLayers.Add(_kmlLayer);
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hide the base UI and enable the complete button.
                ShapesPanel.Visibility = Visibility.Collapsed;
                CompleteButton.Visibility = Visibility.Visible;
                SaveResetGrid.Visibility = Visibility.Collapsed;

                // Create variables for the sketch creation mode and color.
                SketchCreationMode creationMode;

                // Set the creation mode and UI based on which button called this method.
                switch (((Button)sender).Name)
                {
                    case nameof(PointButton):
                        creationMode = SketchCreationMode.Point;
                        InstructionsText.Text = "Tap to add a point.";
                        StyleText.Text = "Select an icon for the placemark.";
                        break;

                    case nameof(PolylineButton):
                        creationMode = SketchCreationMode.Polyline;
                        InstructionsText.Text = "Tap to add a vertex.";
                        StyleText.Text = "Select a color for the placemark.";
                        break;

                    case nameof(PolygonButton):
                        creationMode = SketchCreationMode.Polygon;
                        InstructionsText.Text = "Tap to add a vertex.";
                        StyleText.Text = "Select a color for the placemark.";
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

                // Enable the style editing UI.
                StyleBorder.Visibility = Visibility.Visible;
                MainUI.Visibility = Visibility.Collapsed;

                // Display the Icon picker or the color picker based on the creation mode.
                IconPicker.Visibility = creationMode == SketchCreationMode.Point ? Visibility.Visible : Visibility.Collapsed;
                ColorSelector.Visibility = creationMode != SketchCreationMode.Point ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (ArgumentException)
            {
                await new MessageDialog("Unsupported Geometry", "Error").ShowAsync();
            }
            finally
            {
                // Reset the UI.
                ShapesPanel.Visibility = Visibility.Visible;
                CompleteButton.Visibility = Visibility.Collapsed;
                InstructionsText.Text = "Select the type of feature you would like to add.";

                // Enable the save and reset buttons.
                SaveResetGrid.Visibility = Visibility;
            }
        }

        private void Apply_Style_Click(object sender, RoutedEventArgs e)
        {
            // Create a new style for the placemark.
            _currentPlacemark.Style = new KmlStyle();

            // Get the color from the ColorSelector
            var selected = ColorSelector.Color;
            Color color = Color.FromArgb(selected.A, selected.R, selected.G, selected.B);

            // Set the style for that Placemark.
            switch (_currentPlacemark.GraphicType)
            {
                // Create a KmlIconStyle using the selected icon.
                case KmlGraphicType.Point:
                    _currentPlacemark.Style.IconStyle = new KmlIconStyle(new KmlIcon((Uri)IconPicker.SelectedItem), 1.0);
                    break;

                // Create a KmlLineStyle using the selected color value.
                case KmlGraphicType.Polyline:
                    _currentPlacemark.Style.LineStyle = new KmlLineStyle(color, 8);
                    break;

                // Create a KmlPolygonStyle using the selected color value.
                case KmlGraphicType.Polygon:
                    _currentPlacemark.Style.PolygonStyle = new KmlPolygonStyle(color);
                    _currentPlacemark.Style.PolygonStyle.IsFilled = true;
                    _currentPlacemark.Style.PolygonStyle.IsOutlined = false;
                    break;
            }

            // Reset the UI.
            StyleBorder.Visibility = Visibility.Collapsed;
            MainUI.Visibility = Visibility.Visible;
        }

        private void No_Style_Click(object sender, RoutedEventArgs e)
        {
            // Reset the UI.
            StyleBorder.Visibility = Visibility.Collapsed;
            MainUI.Visibility = Visibility.Visible;
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Open a save dialog for the user.
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("KMZ file", new List<string>() { ".kmz" });
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                try
                {
                    using (Stream stream = await file.OpenStreamForWriteAsync())
                    {
                        // Write the KML document to the stream of the file.
                        await _kmlDocument.WriteToAsync(stream);
                    }
                    await new MessageDialog("Item saved.").ShowAsync();
                }
                catch
                {
                    await new MessageDialog("File not saved.").ShowAsync();
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetKml();
        }
    }
}