// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinUI = Windows.UI;
using Windows.UI.Popups;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Esri.ArcGISRuntime.UI;

namespace ArcGISRuntime.UWP.Samples.AuthorMap
{
    public partial class AuthorMap
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with
        private string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server
        private string AppClientId = "2Gh53JRzkPtOENQq";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private string OAuthRedirectUrl = "https://developers.arcgis.com";

        // String array to store names of the available basemaps
        private string[] _basemapNames = new string[]
        {
            "Light Gray",
            "Topographic",
            "Streets",
            "Imagery",
            "Ocean"
        };

        // Dictionary of operational layer names and URLs
        private Dictionary<string, string> _operationalLayerUrls = new Dictionary<string, string>
        {
            {"World Elevations", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer"},
            {"World Cities", "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/" },
            {"US Census Data", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer"}
        };

        public AuthorMap()
        {
            InitializeComponent();

            // When the map view loads, show a dialog for entering OAuth settings
            MyMapView.Loaded += (s,e) => ShowOAuthSettingsDialog();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            BasemapListView.ItemsSource = _basemapNames;
            LayerListView.ItemsSource = _operationalLayerUrls;

            // Show a plain gray map in the map view
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());
            
            // Update the extent labels whenever the view point (extent) changes
            MyMapView.ViewpointChanged += (s, evt) => UpdateViewExtentLabels();
        }

        #region UI event handlers
        private void BasemapItemClick(object sender, RoutedEventArgs e)
        {
            // Get the name of the desired basemap 
            var radioBtn = sender as RadioButton;
            var basemapName = radioBtn.Content.ToString();

            // Apply the basemap to the current map
            ApplyBasemap(basemapName);
        }

        private void LayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Call a function to add operational layers to the map
            AddOperationalLayers();
        }

        private async void SaveMapClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Don't attempt to save if the OAuth settings weren't provided
                if(string.IsNullOrEmpty(AppClientId) || string.IsNullOrEmpty(OAuthRedirectUrl))
                {
                    var dialog = new MessageDialog("OAuth settings were not provided.", "Cannot Save");
                    await dialog.ShowAsync();

                    SaveMapFlyout.Hide();

                    return;
                }

                // Show the progress bar so the user knows work is happening
                SaveProgressBar.Visibility = Visibility.Visible;

                // Get the current map
                var myMap = MyMapView.Map;

                // Apply the current extent as the map's initial extent
                myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // See if the map has already been saved (has an associated portal item)
                if (myMap.Item == null)
                {
                    // Get information for the new portal item
                    var title = TitleTextBox.Text;
                    var description = DescriptionTextBox.Text;
                    var tagText = TagsTextBox.Text;

                    // Make sure all required info was entered
                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(tagText))
                    {
                        throw new Exception("Please enter a title, description, and some tags to describe the map.");
                    }

                    // Call a function to save the map as a new portal item
                    await SaveNewMapAsync(MyMapView.Map, title, description, tagText.Split(','));

                    // Report a successful save
                    var messageDialog = new MessageDialog("Saved '" + title + "' to ArcGIS Online!", "Map Saved");
                    await messageDialog.ShowAsync();
                }
                else
                {
                    // This is not the initial save, call SaveAsync to save changes to the existing portal item
                    await myMap.SaveAsync();

                    // Report update was successful
                    var messageDialog = new MessageDialog("Saved changes to '" + myMap.Item.Title + "'", "Updates Saved");
                    await messageDialog.ShowAsync();
                }

                // Update the portal item thumbnail with the current map image
                try
                {
                    // Export the current map view
                    var mapImage = await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(await MyMapView.ExportImageAsync());

                    // Call a function that writes a temporary jpeg file of the map
                    var imagePath = await WriteTempThumbnailImageAsync(mapImage);

                    // Call a function to update the portal item's thumbnail with the image
                    UpdatePortalItemThumbnailAsync(imagePath);
                }
                catch
                {
                    // Throw an exception to let the user know the thumbnail was not saved (the map item was)
                    throw new Exception("Thumbnail was not updated.");
                }
            }
            catch (Exception ex)
            {
                // Report error message
                var messageDialog = new MessageDialog("Error saving map to ArcGIS Online: " + ex.Message);
                await messageDialog.ShowAsync();
            }
            finally
            {
                // Hide the progress bar
                SaveProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearMapClicked(object sender, RoutedEventArgs e)
        {
            // Create a new map (will not have an associated PortalItem)
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());
        }
        #endregion

        private void ApplyBasemap(string basemapName)
        {
            // Get the current map
            Map myMap = MyMapView.Map;

            // Set the basemap for the map according to the user's choice in the list box
            switch (basemapName)
            {
                case "Light Gray":
                    // Set the basemap to Light Gray Canvas
                    myMap.Basemap = Basemap.CreateLightGrayCanvas();
                    break;
                case "Topographic":
                    // Set the basemap to Topographic
                    myMap.Basemap = Basemap.CreateTopographic();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    myMap.Basemap = Basemap.CreateStreets();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    myMap.Basemap = Basemap.CreateImagery();
                    break;
                case "Ocean":
                    // Set the basemap to Oceans
                    myMap.Basemap = Basemap.CreateOceans();
                    break;
                default:
                    break;
            }
        }

        private void AddOperationalLayers()
        {
            // Clear the operational layers from the map
            Map myMap = MyMapView.Map;
            myMap.OperationalLayers.Clear();

            // Loop through the selected items in the operational layers list box
            foreach (var item in LayerListView.SelectedItems)
            {
                // Get the service uri for each selected item 
                var layerInfo = (KeyValuePair<string, string>)item;
                var layerUri = new Uri(layerInfo.Value);

                // Create a new map image layer, set it 50% opaque, and add it to the map
                ArcGISMapImageLayer layer = new ArcGISMapImageLayer(layerUri);
                layer.Opacity = 0.5;
                myMap.OperationalLayers.Add(layer);
            }
        }

        private async Task SaveNewMapAsync(Map myMap, string title, string description, string[] tags)
        {
            // Challenge the user for portal credentials (OAuth credential request for arcgis.com)
            CredentialRequestInfo loginInfo = new CredentialRequestInfo();

            // Use the OAuth implicit grant flow
            loginInfo.GenerateTokenOptions = new GenerateTokenOptions
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Indicate the url (portal) to authenticate with (ArcGIS Online)
            loginInfo.ServiceUri = new Uri("http://www.arcgis.com/sharing/rest");

            try
            {
                // Get a reference to the (singleton) AuthenticationManager for the app
                AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await thisAuthenticationManager.GetCredentialAsync(loginInfo, false);
            }
            catch (OperationCanceledException)
            {
                // user canceled the login
                throw new Exception("Portal log in was canceled.");
            }

            // Get the ArcGIS Online portal (will use credential from login above)
            ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync();

            // Export the current map view to use as the item's thumbnail
            RuntimeImage img = await MyMapView.ExportImageAsync();

            // Save the current state of the map as a portal item in the user's default folder
            await myMap.SaveAsAsync(agsOnline, null, title, description, tags, img, false);
        }

        private async void UpdatePortalItemThumbnailAsync(string imageFileName)
        {
            // Update the portal item with a thumbnail image of the current map
            try
            {
                // Get the map's portal item
                PortalItem newPortalItem = MyMapView.Map.Item as PortalItem;

                // Open the image file (stored in the device's Pictures folder)
                var mapImageFile = await KnownFolders.PicturesLibrary.GetFileAsync(imageFileName);

                if (mapImageFile != null)
                {
                    // Get a thumbnail image (scaled down version) of the original
                    var thumbnailData = await mapImageFile.GetScaledImageAsThumbnailAsync(0);

                    // Assign the thumbnail data (file stream) to the content object
                    newPortalItem.SetThumbnailWithImage(thumbnailData.AsStreamForRead());

                    // Update the portal item with the new content (just the thumbnail will be updated)
                    await newPortalItem.UpdateItemPropertiesAsync();

                    // Delete the map image file from disk
                    mapImageFile.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                // Warn the user that the thumbnail could not be updated
                var dialog = new MessageDialog("Unable to update thumbnail for portal item: " + ex.Message, "Portal Item Thumbnail");
                dialog.ShowAsync();
            }
        }

        private async Task<string> WriteTempThumbnailImageAsync(ImageSource mapImageSource)
        {
            string outputFilename = string.Empty;

            try
            {
                // Export the current map view display to a bitmap
                var mapImage = mapImageSource as WriteableBitmap;

                // Create a new file in the device's Pictures folder
                var outStorageFile = await KnownFolders.PicturesLibrary.CreateFileAsync("MapImage_Temp.jpg", CreationCollisionOption.GenerateUniqueName);
                outputFilename = outStorageFile.Name;

                // Open the new file for read/write
                using (var stream = await outStorageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Create a bitmap encoder to encode the image to Jpeg
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                    // Read the pixels from the map image into a byte array
                    var pixelStream = mapImage.PixelBuffer.AsStream();
                    var pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    // Use the encoder to write the map image pixels to the output file
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)mapImage.PixelWidth, (uint)mapImage.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();
                }
            }
            catch
            {
                // Exception message will be shown in the calling function
                throw;
            }

            return outputFilename;
        }

        private void UpdateViewExtentLabels()
        {
            // Get the current view point for the map view
            Viewpoint currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            if (currentViewpoint == null) { return; }

            // Get the current map extent (envelope) from the view point
            Envelope currentExtent = currentViewpoint.TargetGeometry as Envelope;

            // Project the current extent to geographic coordinates (longitude / latitude)
            Envelope currentGeoExtent = GeometryEngine.Project(currentExtent, SpatialReferences.Wgs84) as Envelope;

            // Fill the app text boxes with min / max longitude (x) and latitude (y) to four decimal places
            XMinTextBox.Text = currentGeoExtent.XMin.ToString("0.####");
            YMinTextBox.Text = currentGeoExtent.YMin.ToString("0.####");
            XMaxTextBox.Text = currentGeoExtent.XMax.ToString("0.####");
            YMaxTextBox.Text = currentGeoExtent.YMax.ToString("0.####");
        }

        #region OAuth helpers
        private async void ShowOAuthSettingsDialog()
        {
            // Show default settings for client ID and redirect URL
            ClientIdTextBox.Text = AppClientId;
            RedirectUrlTextBox.Text = OAuthRedirectUrl;

            // Display inputs for a client ID and redirect URL to use for OAuth authentication
            ContentDialogResult result = await OAuthSettingsDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Settings were provided, update the configuration settings for OAuth authorization
                AppClientId = ClientIdTextBox.Text.Trim();
                OAuthRedirectUrl = RedirectUrlTextBox.Text.Trim();

                // Update authentication manager with the OAuth settings
                UpdateAuthenticationManager();
            }
            else
            {
                // User canceled, warn that won't be able to save
                var messageDlg = new MessageDialog("No OAuth settings entered, you will not be able to save your map.");
                await messageDlg.ShowAsync();

                AppClientId = string.Empty;
                OAuthRedirectUrl = string.Empty;
            }
        }

        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                // Otherwise, use OAuthImplicit
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the server information
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        // ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
        // resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            OAuthTokenCredential credential = null;

            try
            {
                // Create generate token options if necessary
                if (info.GenerateTokenOptions == null)
                {
                    info.GenerateTokenOptions = new GenerateTokenOptions();
                }

                // IOAuthAuthorizeHandler will challenge the user for credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync
                    (
                            info.ServiceUri,
                            info.GenerateTokenOptions
                    ) as OAuthTokenCredential;
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw (ex);
            }

            return credential;
        }
        #endregion
    }
}
