// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ArcGISRuntimeXamarin.Samples.ListTransformations
{
    [Activity]
    public class ListTransformations : Activity
    {
        // Map view control to display a map in the app.
        private MapView _myMapView = new MapView();
        
        // Point whose coordinates will be projected using a selected transform.
        private MapPoint _originalPoint;

        // Graphic representing the projected point.
        private Graphic _projectedPointGraphic;

        // GraphicsOverlay to hold the point graphics.
        private GraphicsOverlay _pointsOverlay;

        // Text view to display messages to the user (exceptions, etc.).
        private TextView _messagesTextView;

        // Labels to display the input/output spatial references (WKID).
        private TextView _inWkidLabel;
        private TextView _outWkidLabel;

        // Spinner to display the datum transformations suitable for the input/output spatial references.
        private Spinner _transformationsPicker;

        // Switch to toggle suitable transformations for the current extent.
        private Switch _useExtentSwitch;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "List transformations";

            // Create the UI.
            CreateLayout();

            // Create a new map, add a point graphic, and fill the datum transformations list.
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map and add it to the map view control.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Create a point in the Greenwich observatory courtyard in London, UK, the location of the prime meridian. 
            _originalPoint = new MapPoint(538985.355, 177329.516, SpatialReference.Create(27700));

            // Set the initial extent to an extent centered on the point.
            Viewpoint initialViewpoint = new Viewpoint(_originalPoint, 5000);
            myMap.InitialViewpoint = initialViewpoint;

            // Handle the map loading to fill the UI controls.
            myMap.Loaded += MyMap_Loaded;

            // Load the map and add the map to the map view.
            await myMap.LoadAsync();
            _myMapView.Map = myMap;

            // Create a graphics overlay to hold the original and projected points.
            _pointsOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_pointsOverlay);

            // Add the point as a graphic with a blue square.
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.Blue, 15);
            Graphic originalGraphic = new Graphic(_originalPoint, markerSymbol);
            _pointsOverlay.Graphics.Add(originalGraphic);

            // Get the path to the projection engine data (if it exists).
            string peFolderPath = GetProjectionDataPath();
            if (!string.IsNullOrEmpty(peFolderPath))
            {
                TransformationCatalog.ProjectionEngineDirectory = peFolderPath;
                _messagesTextView.Text = "Using projection data found at '" + peFolderPath + "'";
            }
            else
            {
                _messagesTextView.Text = "Projection engine data not found.";
            }
        }

        private void MyMap_Loaded(object sender, EventArgs e)
        {
            // Get the map's spatial reference.
            SpatialReference mapSpatialReference = (sender as Map).SpatialReference;

            // Run on the UI thread.
            RunOnUiThread(() =>
            {
                // Show the input and output spatial reference (WKID) in the labels.
                _inWkidLabel.Text = "In WKID = " + _originalPoint.SpatialReference.Wkid;
                _outWkidLabel.Text = "Out WKID = " + mapSpatialReference.Wkid;

                // Call a function to create a list of transformations to fill the picker.
                GetSuitableTransformations(_originalPoint.SpatialReference, mapSpatialReference, _useExtentSwitch.Checked);
            });
        }

        private void CreateLayout()
        {
            // View for the input/output wkid labels.
            LinearLayout wkidLabelsStackView = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            
            // Create a label for the input spatial reference.
            _inWkidLabel = new TextView(this)
            {
                Text = "In WKID = ",
                TextAlignment = Android.Views.TextAlignment.ViewStart
            };

            // Create a label for the output spatial reference.
            _outWkidLabel = new TextView(this)
            {
                Text = "Out WKID = ",
                TextAlignment = Android.Views.TextAlignment.ViewStart
            };

            // Create some horizontal space
            Space space = new Space(this);
            space.SetPadding(20, 0, 0, 0);

            // Add the Wkid labels to the stack view.
            wkidLabelsStackView.AddView(_inWkidLabel);
            wkidLabelsStackView.AddView(space);
            wkidLabelsStackView.AddView(_outWkidLabel);

            // Create a horizontal stack view for the 'use extent' switch and label.
            LinearLayout extentSwitchRow = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            _useExtentSwitch = new Switch(this)
            {
                Checked = false,
                Text = "Use extent"
            };
            _useExtentSwitch.CheckedChange += UseExtentSwitch_CheckedChange;
            
            // Add the switch to the horizontal stack view.
            extentSwitchRow.AddView(_useExtentSwitch);

            // Create a picker (Spinner) for datum transformations.
            _transformationsPicker = new Spinner(this);

            // Handle the selection event to work with the selected transformation.
            _transformationsPicker.ItemSelected += TransformationsPicker_ItemSelected;

            // Create a text view to show messages.
            _messagesTextView = new TextView(this);

            // Create a new vertical layout for the app UI.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the transformation UI controls to the main layout.
            layout.AddView(wkidLabelsStackView);
            layout.AddView(extentSwitchRow);
            layout.AddView(_transformationsPicker);
            layout.AddView(_messagesTextView);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        // Function to get suitable datum transformations for the specified input and output spatial references.
        private void GetSuitableTransformations(SpatialReference inSpatialRef, SpatialReference outSpatialRef, bool considerExtent)
        {
            // Get suitable transformations. Use the current extent to evaluate suitability, if requested.
            IReadOnlyList<DatumTransformation> transformations;
            if (considerExtent)
            {
                Envelope currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
                transformations = TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef, currentExtent);
            }
            else
            {
                transformations = TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef);
            }

            // Get the default transformation for the specified input and output spatial reference.
            DatumTransformation defaultTransform = TransformationCatalog.GetTransformation(inSpatialRef, outSpatialRef);

            // Create a list of transformations.
            List<DatumTransformation> transformsList = new List<DatumTransformation>();
            foreach(DatumTransformation transformation in transformations)
            {
                transformsList.Add(transformation);
            }

            // Create an adapter for showing the spinner list.
            TransformationsAdapter transformationsAdapter = new TransformationsAdapter(this, transformsList);
            transformationsAdapter.DefaultTransformation = defaultTransform;

            // Apply the adapter to the spinner.
            _transformationsPicker.Adapter = transformationsAdapter;
        }

        private void TransformationsPicker_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            
        }

        private void UseExtentSwitch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            
        }

        private string GetProjectionDataPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder path.
            string folder = DataManager.GetDataFolder();

            // Get the full path to the projection engine data folder.
            string folderPath = Path.Combine(folder, "SampleData", "PEDataRuntime");

            // Check if the directory exists.
            if (!Directory.Exists(folderPath))
            {
                folderPath = "";
            }

            return folderPath;

            #endregion offlinedata
        }
    }

    public class TransformationsAdapter : BaseAdapter<DatumTransformation>
    {
        private List<DatumTransformation> _transformations;
        private Activity _context;
        public DatumTransformation DefaultTransformation { get; set; }

        public TransformationsAdapter(Activity context, List<DatumTransformation> items) : base()
        {
            _transformations = items;
            _context = context;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override DatumTransformation this[int position]
        {
            get { return _transformations[position]; }
        }

        public override int Count
        {
            get
            {
                return _transformations.Count;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            DatumTransformation thisTransform = _transformations[position];

            TextView view = new TextView(_context);
            view.SetTextColor(Android.Graphics.Color.White);
            if (thisTransform.IsMissingProjectionEngineFiles)
            {
                view.SetTextColor(Android.Graphics.Color.Gray);
            }

            if(thisTransform.Name == DefaultTransformation.Name)
            {
                view.SetTextColor(Android.Graphics.Color.Blue);
            }
            
            return view;
        }
    }
}