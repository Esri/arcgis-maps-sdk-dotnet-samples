﻿// Copyright 2022 Esri.
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
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using System.Globalization;
using Color = System.Drawing.Color;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;
using Map = Esri.ArcGISRuntime.Mapping.Map;

#if WINDOWS
using Windows.Storage.Pickers;
#endif

namespace ArcGIS.Samples.CreateAndSaveKmlFile
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create and save KML file",
        category: "Layers",
        description: "Construct a KML document and save it as a KMZ file.",
        instructions: "Tap on one of the buttons in the middle row to start adding a geometry. Tap on the map view to place vertices. Tap the \"Complete Sketch\" button to add the geometry to the KML document as a new KML placemark. Use the style interface to edit the style of the placemark. If you do not wish to set a style, tap the \"Don't Apply Style\" button. When you are finished adding KML nodes, tap on the \"Save KMZ file\" button to save the active KML document as a .kmz file on your system. Use the \"Reset\" button to clear the current KML document and start a new one.",
        tags: new[] { "KML", "KMZ", "Keyhole", "OGC", "geometry editor" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    [ArcGIS.Samples.Shared.Attributes.ClassFile("Converters/ImageConverter.cs", "Converters/ColorConverter.cs")]
    public partial class CreateAndSaveKmlFile : ContentPage
    {
        private KmlDocument _kmlDocument;
        private KmlDataset _kmlDataset;
        private KmlLayer _kmlLayer;
        private KmlPlacemark _currentPlacemark;
        private GeometryType _geometryType;

        public CreateAndSaveKmlFile()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

            List<string> colorHexes = new List<string>()
            {
                "#000000","#FFFFFF","#FF0000","#00FF00","#0000FF","#FFFF00", "#00FFFF","#FF00FF", "#C0C0C0", "#808080","#800000", "#808000","#008000", "#800080","#008080","#000080"
            };
            ColorPicker.ItemsSource = colorHexes;

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

            // Create a new KML document.
            _kmlDocument = new KmlDocument() { Name = "KML Sample Document" };

            // Create a KML dataset using the KML document.
            _kmlDataset = new KmlDataset(_kmlDocument);

            // Create the KML layer using the KML dataset.
            _kmlLayer = new KmlLayer(_kmlDataset);

            // Add the KML layer to the map.
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

                // Set the creation mode and UI based on which button called this method.
                switch (((Button)sender).Text)
                {
                    case "Point":
                        _geometryType = GeometryType.Point;
                        Status.Text = "Tap to add a point.";
                        break;

                    case "Polyline":
                        _geometryType = GeometryType.Polyline;
                        Status.Text = "Tap to add a vertex.";
                        break;

                    case "Polygon":
                        _geometryType = GeometryType.Polygon;
                        Status.Text = "Tap to add a vertex.";
                        break;

                    default:
                        return;
                }

                // Start the geometry editor.
                MyMapView.GeometryEditor.Start(_geometryType);
            }
            catch (ArgumentException)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Unsupported Geometry", "OK");
            }
        }

        private void Apply_Style_Click(object sender, SelectedItemChangedEventArgs e)
        {
            // Get the color value if the selected item is a hexadecimal color.
            Color systemColor = Color.Transparent;
            if (((string)e.SelectedItem).StartsWith('#'))
            {
                Color platColor = Color.FromArgb(Int32.Parse(((string)e.SelectedItem).Replace("#", ""), NumberStyles.HexNumber));
                systemColor = Color.FromArgb(255, (int)(platColor.R), (int)(platColor.G), (int)(platColor.B));
            }

            // Create a new style for the placemark.
            _currentPlacemark.Style = new KmlStyle();

            // Set the style for that Placemark.
            switch (_currentPlacemark.GraphicType)
            {
                // Create a KmlIconStyle using the selected icon.
                case KmlGraphicType.Point:
                    Uri iconLink = new Uri((string)e.SelectedItem);
                    _currentPlacemark.Style.IconStyle = new KmlIconStyle(new KmlIcon(iconLink), 1.0);
                    break;

                // Create a KmlLineStyle using the selected color value.
                case KmlGraphicType.Polyline:
                    _currentPlacemark.Style.LineStyle = new KmlLineStyle(systemColor, 8);
                    break;

                // Create a KmlPolygonStyle using the selected color value.
                case KmlGraphicType.Polygon:
                    _currentPlacemark.Style.PolygonStyle = new KmlPolygonStyle(systemColor);
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
                // Get the user-drawn geometry.
                Geometry geometry = MyMapView.GeometryEditor.Stop();

                // Hold a reference for the new placemark geometry.
                KmlGeometry kmlGeometry;

                // Check to see if a geometry has been drawn.
                if (!geometry.IsEmpty)
                {

                    if (MyMapView.SpatialReference != null &&
                        geometry.SpatialReference != MyMapView.SpatialReference)
                    {
                        // Project the geometry to WGS84 (WGS84 is required by the KML standard).
                        Geometry projectedGeometry = geometry.Project(SpatialReferences.Wgs84);

                        // Create a KmlGeometry using the projected geometry.
                        kmlGeometry = new KmlGeometry(projectedGeometry, KmlAltitudeMode.ClampToGround);
                    }
                    else
                    {
                        // Create a KmlGeometry using the user-drawn geometry.
                        kmlGeometry = new KmlGeometry(geometry, KmlAltitudeMode.ClampToGround);
                    }

                    // Create a new placemark.
                    _currentPlacemark = new KmlPlacemark(kmlGeometry);

                    // Add the placemark to the KmlDocument.
                    _kmlDocument.ChildNodes.Add(_currentPlacemark);

                    // Choose whether to enable the icon picker or color picker.
                    IconPicker.IsVisible = _geometryType == GeometryType.Point;
                    ColorPicker.IsVisible = _geometryType != GeometryType.Point;

                    // Enable the style editing UI.
                    StyleUI.IsVisible = true;
                    MainUI.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        private async void Save_Click(object sender, EventArgs e)
        {
            try
            {
                // Get permission to write to storage.
                PermissionStatus writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (writeStatus != PermissionStatus.Granted)
                {
                    throw new Exception("Storage writing permission is required to save file.");
                }

#if IOS || MACCATALYST
                // Determine the path for the file.
                string offlineDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CreateAndSaveKmlFile");

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

                await ShareFile(path);
#elif ANDROID
                // Determine the path for the file.
                string path = Path.Combine(Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath, "sampledata.kmz");

                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Write the KML document to the stream of the file.
                    await _kmlDocument.WriteToAsync(stream);
                }

                await ShareFile(path);
#elif WINDOWS
                // Open a save dialog for the user.
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("KMZ file", new List<string>() { ".kmz" });

                // Get the handle for the main window.
                var windowHandle = ((MauiWinUIWindow)Application.Current.Windows[0].Handler.PlatformView).WindowHandle;

                // Initialize the file picker with the window.
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);

                // Show the picker to the user.
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
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task ShareFile(string path)
        {
            try
            {
                // Share the file using the Maui share feature.
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share geodatabase",
                    File = new ShareFile(path)
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            ResetKml();
        }
    }
}