// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI;
using System;

namespace ArcGISRuntime.Samples.TakeScreenshot
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Take screenshot",
        "MapView",
        "This sample demonstrates how you can take screenshot of a map. Click 'capture' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.",
        "")]
    public class TakeScreenshot : Activity
    {
        // Create and hold reference to the used map view
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Take screenshot";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map instance with the basemap  
            Basemap myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Assign the map to the map view
            _myMapView.Map = myMap;
        }

        private async void OnTakeScreenshotClicked(object sender, EventArgs e)
        {
            try
            {
                // Export the image from map view
                var exportedImage = await _myMapView.ExportImageAsync();

                // Create an image button (this will display the exported map view image)
                var myImageButton = new ImageButton(this);

                // Define the size of the image button to be 2/3 the size of the map view
                myImageButton.LayoutParameters = new Android.Views.ViewGroup.LayoutParams((int)(_myMapView.Width *.667) , (int)(_myMapView.Height * .667));

                // Set the source of the image button to be that of the exported map view image
                myImageButton.SetImageBitmap(await exportedImage.ToImageSourceAsync());

                // Make the image that was captured from the map view export to fit within (aka scale-to-fit) the image button
                myImageButton.SetScaleType(ImageView.ScaleType.FitCenter);

                // Define a popup with a single image button control and make the size of the popup to be 2/3 the size of the map view 
                var myPopupWindow = new PopupWindow(myImageButton, (int)(_myMapView.Width * .667), (int)(_myMapView.Height * .667));

                // Display the popup in the middle of the map view
                myPopupWindow.ShowAtLocation(_myMapView, Android.Views.GravityFlags.Center, 0, 0);

                // Define a lambda event handler to close the popup when the user clicks on the image button 
                myImageButton.Click += (s, a) => myPopupWindow.Dismiss();

            }
            catch (Exception ex)
            {
                // Display any errors to the user if capturing the map view image did not work
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("ExportImageAsync error");
                alertBuilder.SetMessage("Capturing image failed. " + ex.Message);
                alertBuilder.Show();
            }

        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a button to take a screen shot, with wired up event
            var takeScreenshotButton = new Button(this);
            takeScreenshotButton.Text = "Capture";
            takeScreenshotButton.Click += OnTakeScreenshotClicked;
            layout.AddView(takeScreenshotButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}