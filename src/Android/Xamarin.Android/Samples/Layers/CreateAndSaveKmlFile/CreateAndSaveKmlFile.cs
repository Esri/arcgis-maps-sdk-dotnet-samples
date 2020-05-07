// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Resource = ArcGISRuntime.Resource;

namespace ArcGISRuntimeXamarin.Samples.CreateAndSaveKmlFile
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create and save KML file",
        "Layers",
        "Construct a KML document and save it as a KMZ file.",
        "Tap on one of the buttons in the middle row to start adding a geometry. Tap on the map view to place vertices. Tap the \"Complete Sketch\" button to add the geometry to the KML document as a new KML placemark. Use the style interface to edit the style of the placemark. If you do not wish to set a style, tap the \"Don't Apply Style\" button. When you are finished adding KML nodes, tap on the \"Save KMZ file\" button to save the active KML document as a .kmz file on your system. Use the \"Reset\" button to clear the current KML document and start a new one.",
        "KML", "KMZ", "Keyhole", "OGC", "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("CreateAndSaveKmlFile.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class CreateAndSaveKmlFile : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Button _addButton;
        private Button _saveButton;
        private Button _resetButton;
        private Button _completeButton;
        private Button _noStyleButton;
        private LinearLayout _buttonLayout;
        private LinearLayout _pickerLayout;
        private TextView _status;
        private TextView _styleText;
        private ListView _listView;
        private BaseAdapter _iconAdapter;
        private BaseAdapter _colorAdapter;

        private KmlDocument _kmlDocument;
        private KmlDataset _kmlDataset;
        private KmlLayer _kmlLayer;
        private KmlPlacemark _currentPlacemark;

        private List<string> _iconLinks;
        private List<Color> _colorList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Create and save KML file";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Set up a new kml document and kml layer.
            ResetKml();

            // Set the colors for the color picker.
            _colorList = new List<Color>()
            {
               Color.Blue, Color.Brown, Color.Gray, Color.Green, Color.Yellow, Color.Pink, Color.Purple, Color.Red, Color.Black
            };
            _colorAdapter = new ColorAdapter(this, _colorList);

            // Set the images for the point icon picker.
            _iconLinks = new List<string>()
            {
                "https://static.arcgis.com/images/Symbols/Shapes/BlueCircleLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueDiamondLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BluePin1LargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BluePin2LargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueSquareLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueStarLargeB.png"
            };

            // Request permission to save a file.
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, 1);
            }
        }

        private void ResetKml()
        {
            // Clear any existing layers from the map.
            _myMapView.Map.OperationalLayers.Clear();

            // Reset the most recently placed placemark.
            _currentPlacemark = null;

            // Create a new KML document.
            _kmlDocument = new KmlDocument() { Name = "KML Sample Document" };

            // Create a KML dataset using the KML document.
            _kmlDataset = new KmlDataset(_kmlDocument);

            // Create the KML layer using the KML dataset.
            _kmlLayer = new KmlLayer(_kmlDataset);

            // Add the KML layer to the map.
            _myMapView.Map.OperationalLayers.Add(_kmlLayer);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            string[] options = new string[] { "Point", "Polyline", "Polygon" };

            // Create UI for terminal selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetItems(options, Draw);
            builder.SetCancelable(false);
            builder.Show();
        }

        private async void Draw(object sender, DialogClickEventArgs e)
        {
            try
            {
                // Hide the base UI and enable the complete button.
                _buttonLayout.Visibility = ViewStates.Invisible;
                _completeButton.Visibility = ViewStates.Visible;

                // Create variables for the sketch creation mode and color.
                SketchCreationMode creationMode;

                // Set the creation mode and UI based on which button called this method.
                switch (e.Which)
                {
                    case 0:
                        creationMode = SketchCreationMode.Point;
                        _status.Text = "Tap to add a point.";
                        //StyleText.Text = "Select an icon for the placemark.";
                        break;

                    case 1:
                        creationMode = SketchCreationMode.Polyline;
                        _status.Text = "Tap to add a vertex.";
                        //StyleText.Text = "Select a color for the placemark.";
                        break;

                    case 2:
                        creationMode = SketchCreationMode.Polygon;
                        _status.Text = "Tap to add a vertex.";
                        //StyleText.Text = "Select a color for the placemark.";
                        break;

                    default:
                        return;
                }

                // Get the user-drawn geometry.
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, true);

                // Project the geometry to WGS84 (WGS84 is required by the KML standard).
                Geometry projectedGeometry = GeometryEngine.Project(geometry, SpatialReferences.Wgs84);

                // Create a KmlGeometry using the new geometry.
                KmlGeometry kmlGeometry = new KmlGeometry(projectedGeometry, KmlAltitudeMode.ClampToGround);

                // Create a new placemark.
                _currentPlacemark = new KmlPlacemark(kmlGeometry);

                // Add the placemark to the KmlDocument.
                _kmlDocument.ChildNodes.Add(_currentPlacemark);

                // Choose whether to open the icon picker or color picker.
                if (creationMode == SketchCreationMode.Point) { OpenIconDialog(); }
                else { OpenColorDialog(); }
            }
            catch (ArgumentException)
            {
                new AlertDialog.Builder(this).SetMessage("Unsupported Geometry").SetTitle("Error").Show();
            }
            finally
            {
                // Reset the UI.
                _buttonLayout.Visibility = ViewStates.Visible;
                _completeButton.Visibility = ViewStates.Invisible;
                _status.Text = "";
            }
        }

        private void Complete_Click(object sender, EventArgs e)
        {
            try
            {
                // Finish the sketch.
                _myMapView.SketchEditor.CompleteCommand.Execute(null);
            }
            catch (ArgumentException)
            {
            }
        }

        private void OpenIconDialog()
        {
            // Create the icon adapter if it has not been created yet.
            if (_iconAdapter == null) { _iconAdapter = new IconAdapter(this, _iconLinks); }

            // Display the picker view.
            _listView.Adapter = _iconAdapter;
            _styleText.Text = "Select an icon.";
            _pickerLayout.Visibility = ViewStates.Visible;
            _listView.ItemClick += IconSelected;
        }

        private void IconSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Set the style of the placemark.
            _currentPlacemark.Style = new KmlStyle();
            _currentPlacemark.Style.IconStyle = new KmlIconStyle(new KmlIcon(new Uri(_iconLinks[(int)e.Id])), 1.0);

            // Reset the UI.
            _pickerLayout.Visibility = ViewStates.Invisible;
            _listView.ItemClick -= IconSelected;
        }

        private void OpenColorDialog()
        {
            // Display the picker view.
            _listView.Adapter = _colorAdapter;
            _styleText.Text = "Select a color.";
            _pickerLayout.Visibility = ViewStates.Visible;
            _listView.ItemClick += ColorSelected;
        }

        private void ColorSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Convert the selected Android color into a System.Drawing.Color.
            Color androidColor = _colorList[e.Position];
            System.Drawing.Color systemColor = System.Drawing.Color.FromArgb(androidColor.R, androidColor.G, androidColor.B);

            // Create a new style for the placemark.
            _currentPlacemark.Style = new KmlStyle();

            // Check the graphic type of the placemark.
            if (_currentPlacemark.GraphicType == KmlGraphicType.Polyline)
            {
                // Set the line style.
                _currentPlacemark.Style.LineStyle = new KmlLineStyle(systemColor, 8);
            }
            else if (_currentPlacemark.GraphicType == KmlGraphicType.Polygon)
            {
                // Set the polygon style.
                _currentPlacemark.Style.PolygonStyle = new KmlPolygonStyle(systemColor);
                _currentPlacemark.Style.PolygonStyle.IsFilled = true;
                _currentPlacemark.Style.PolygonStyle.IsOutlined = false;
            }

            // Reset the UI.
            _pickerLayout.Visibility = ViewStates.Invisible;
            _listView.ItemClick -= ColorSelected;
        }

        private void No_Style_Click(object sender, EventArgs e)
        {
            // Reset the UI.
            _pickerLayout.Visibility = ViewStates.Invisible;
            _listView.ItemClick -= IconSelected;
            _listView.ItemClick -= ColorSelected;
        }

        private async void Save_Click(object sender, EventArgs e)
        {
            // Determine where to save your file
            string filePath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads, "sampledata.kmz");

            // Check if the user can save their file.
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, 1);
            }
            else
            {
                using (Stream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    // Write the KML document to the stream of the file.
                    await _kmlDocument.WriteToAsync(stream);
                }
                new AlertDialog.Builder(this).SetMessage("File saved to " + filePath).SetTitle("Success!").Show();
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            ResetKml();
        }

        private void CreateLayout()
        {
            // Load the layout from the axml resource.
            SetContentView(Resource.Layout.CreateAndSaveKmlFile);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _addButton = FindViewById<Button>(Resource.Id.addButton);
            _saveButton = FindViewById<Button>(Resource.Id.saveButton);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _completeButton = FindViewById<Button>(Resource.Id.completeButton);
            _noStyleButton = FindViewById<Button>(Resource.Id.noStyleButton);
            _buttonLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout);
            _pickerLayout = FindViewById<LinearLayout>(Resource.Id.PickerUI);
            _status = FindViewById<TextView>(Resource.Id.statusLabel);
            _styleText = FindViewById<TextView>(Resource.Id.styleText);
            _listView = FindViewById<ListView>(Resource.Id.listView);

            // Add listeners for all of the buttons.
            _addButton.Click += Add_Click;
            _saveButton.Click += Save_Click;
            _resetButton.Click += Reset_Click;
            _completeButton.Click += Complete_Click;
            _noStyleButton.Click += No_Style_Click;
        }
    }

    // Adapter to display icons in a ListView.
    public class IconAdapter : BaseAdapter<Bitmap>
    {
        public List<Bitmap> iconList;
        private Context context;

        public IconAdapter(Context context, List<string> list)
        {
            this.context = context;
            iconList = new List<Bitmap>();

            foreach (string link in list)
            {
                Bitmap imageBitmap;
                imageBitmap = BitmapFactory.DecodeStream(WebRequest.Create(link).GetResponse().GetResponseStream());
                iconList.Add(imageBitmap);
            }
        }

        public override Bitmap this[int position] => iconList[position];

        public override int Count => iconList.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Create the image.
            ImageView image = new ImageView(this.context);
            image.SetImageBitmap(iconList[position]);
            image.SetMinimumHeight(150);
            image.SetMinimumWidth(150);

            // Create the layout.
            LinearLayout layout = new LinearLayout(context) { Orientation = Orientation.Horizontal };
            layout.AddView(image);
            layout.SetMinimumHeight(150);
            return layout;
        }
    }

    // Adapter to display color options in a ListView.
    public class ColorAdapter : BaseAdapter<Color>
    {
        public List<Color> colorList;
        private Context context;

        public ColorAdapter(Context context, List<Color> list)
        {
            this.context = context;
            colorList = list;
        }

        public override Color this[int position] => throw new NotImplementedException();

        public override int Count => colorList.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Create a simple layout for the color.
            LinearLayout layout = new LinearLayout(context) { Orientation = Orientation.Horizontal };
            layout.SetMinimumHeight(150);
            layout.SetBackgroundColor(colorList[position]);
            return layout;
        }
    }
}