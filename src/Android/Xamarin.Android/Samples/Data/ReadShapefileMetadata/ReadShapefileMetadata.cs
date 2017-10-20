// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System.IO;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.ReadShapefileMetadata
{
    [Activity]
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
            Map streetMap = new Map(Basemap.CreateStreetsVector());

            // Get the path to the downloaded shapefile
            string filepath = await GetShapefilePath();

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Read metadata about the shapefile and display it in the UI
            _shapefileMetadata = myShapefile.Info;

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

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

        private async Task<string> GetShapefilePath()
        {
            #region offlinedata
            // The shapefile will be downloaded from ArcGIS Online
            // The data manager (a component of the sample viewer, *NOT* the runtime
            //     handles the offline data process

            // The desired shapefile is expected to be called "TrailBikeNetwork.shp"
            string filename = "TrailBikeNetwork.shp";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "FeatureLayerShapefile", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the shapefile
                await DataManager.GetData("d98b3e5293834c5f852f13c569930caa", "FeatureLayerShapefile");
            }

            // Return the path
            return filepath;
            #endregion offlinedata
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            
            // Add a map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Add a button to show the metadata
            Button showMetadataButton = new Button(this);
            showMetadataButton.Text = "Metadata";
            showMetadataButton.Click += ShowMetadataDialog;
            layout.AddView(showMetadataButton);

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
        ShapefileInfo _metadata;
        ImageView _thumbnailImageView;

        public MetadataDialogFragment(ShapefileInfo metadata)
        {
            _metadata = metadata;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Dialog to display
            LinearLayout dialogView = null;

            // Get the context for creating the dialog controls
            Android.Content.Context ctx = this.Activity.ApplicationContext;

            // Set a dialog title
            this.Dialog.SetTitle("Shapefile Metadata");

            base.OnCreateView(inflater, container, savedInstanceState);

            // The container for the dialog is a vertical linear layout
            dialogView = new LinearLayout(ctx);
            dialogView.Orientation = Orientation.Vertical;

            // Add a text box for showing metadata credits
            TextView creditsTextView = new TextView(ctx);
            creditsTextView.Text = _metadata.Credits;
            dialogView.AddView(creditsTextView);

            // Add a text box for showing metadata summary
            TextView summaryTextView = new TextView(ctx);
            summaryTextView.Text = _metadata.Summary;
            dialogView.AddView(summaryTextView);

            // Add an image to show the thumbnail
            _thumbnailImageView = new ImageView(ctx);
            _thumbnailImageView.SetMaxHeight(160);
            _thumbnailImageView.SetMaxWidth(160);
            dialogView.AddView(_thumbnailImageView);

            // Call a function to load the thumbnail image from the metadata
            LoadThumbnail();

            // Add a button to hide the dialog
            Button dismissButton = new Button(ctx);
            dismissButton.Text = "OK";
            //dismissButton.Click += SearchMapsButtonClick;
            dialogView.AddView(dismissButton);

            // Return the new view for display
            return dialogView;
        }

        private async void LoadThumbnail()
        {
            var img = await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(_metadata.Thumbnail);
            _thumbnailImageView.SetImageBitmap(img);
        }
    }
}