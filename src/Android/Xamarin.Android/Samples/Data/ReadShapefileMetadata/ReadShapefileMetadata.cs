// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using ContextThemeWrapper = AndroidX.AppCompat.View.ContextThemeWrapper;

namespace ArcGISRuntime.Samples.ReadShapefileMetadata
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Read shapefile metadata",
        category: "Data",
        description: "Read a shapefile and display its metadata.",
        instructions: "The shapefile's metadata will be displayed when you open the sample.",
        tags: new[] { "credits", "description", "metadata", "package", "shape file", "shapefile", "summary", "symbology", "tags", "visualization" })]
    public class ReadShapefileMetadata : Activity
    {
        // Store the app's map view
        private MapView _myMapView;

        // Store the object that holds the shapefile metadata
        private ShapefileInfo _shapefileMetadata;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Read shapefile metadata";
            CreateLayout();

            // Download (if necessary) and add a local shapefile dataset to the map
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            Map streetMap = new Map(Basemap.CreateStreets());

            // Get the path to the downloaded shapefile
            string filepath = GetShapefilePath();

            try
            {
                // Open the shapefile
                ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

                // Read metadata about the shapefile and display it in the UI
                _shapefileMetadata = myShapefile.Info;

                // Create a feature layer to display the shapefile
                FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);
                await newFeatureLayer.LoadAsync();

                // Zoom the map to the extent of the shapefile
                _myMapView.SpatialReferenceChanged += async (s, e) =>
                {
                    await _myMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);
                };

                // Add the feature layer to the map
                streetMap.OperationalLayers.Add(newFeatureLayer);

                // Show the map in the MapView
                _myMapView.Map = streetMap;
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private static string GetShapefilePath()
        {
            return DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "TrailBikeNetwork.shp");
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a button to show the metadata
            Button showMetadataButton = new Button(this)
            {
                Text = "Show Metadata"
            };
            showMetadataButton.Click += ShowMetadataDialog;
            layout.AddView(showMetadataButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void ShowMetadataDialog(object sender, System.EventArgs e)
        {
            MetadataDialogFragment metadataDialog = new MetadataDialogFragment(_shapefileMetadata);

            // Begin a transaction to show a UI fragment (the metadata dialog)
            FragmentTransaction trans = FragmentManager.BeginTransaction();
            metadataDialog.Show(trans, "metadata");
        }
    }

    // A custom DialogFragment class to show shapefile metadata
    public class MetadataDialogFragment : DialogFragment
    {
        private ShapefileInfo _metadata;
        private ImageView _thumbnailImageView;

        public MetadataDialogFragment(ShapefileInfo metadata)
        {
            _metadata = metadata;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Dialog to display
            LinearLayout dialogView = null;

            // Get the context for creating the dialog controls
            Android.Content.Context ctx = Activity.ApplicationContext;
            AndroidX.AppCompat.View.ContextThemeWrapper ctxWrapper = new ContextThemeWrapper(ctx, Android.Resource.Style.ThemeMaterialLight);

            // Set a dialog title
            Dialog.SetTitle(_metadata.Credits);

            // The container for the dialog is a vertical linear layout
            dialogView = new LinearLayout(ctxWrapper)
            {
                Orientation = Orientation.Vertical
            };
            dialogView.SetPadding(20, 20, 20, 20);

            // Add a text box for showing metadata summary
            TextView summaryTextView = new TextView(ctxWrapper)
            {
                Text = _metadata.Summary
            };
            dialogView.AddView(summaryTextView);

            // Add an image to show the thumbnail
            _thumbnailImageView = new ImageView(ctxWrapper);
            _thumbnailImageView.SetMinimumHeight(200);
            _thumbnailImageView.SetMinimumWidth(200);
            dialogView.AddView(_thumbnailImageView);

            // Call a function to load the thumbnail image from the metadata
            LoadThumbnail();

            // Add a text box for showing metadata tags
            TextView tagsTextView = new TextView(ctxWrapper)
            {
                Text = string.Join(",", _metadata.Tags)
            };
            dialogView.AddView(tagsTextView);

            // Add a button to close the dialog
            Button dismissButton = new Button(ctxWrapper)
            {
                Text = "OK"
            };
            dismissButton.Click += (s, e) => Dismiss();
            dialogView.AddView(dismissButton);

            // Return the new view for display
            return dialogView;
        }

        private async void LoadThumbnail()
        {
            try
            {
                Bitmap img = await _metadata.Thumbnail.ToImageSourceAsync();
                _thumbnailImageView.SetImageBitmap(img);
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(Context).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }
    }
}