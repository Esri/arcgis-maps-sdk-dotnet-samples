// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Helpers;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;

namespace ArcGIS.Samples.SearchPortalMaps
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Search for webmap",
        category: "Map",
        description: "Find webmap portal items by using a search term.",
        instructions: "Enter search terms into the search bar. Once the search is complete, a list is populated with the resultant webmaps. Tap on a webmap to set it to the map view. Scrolling to the bottom of the webmap recycler view will get more results.",
        tags: new[] { "keyword", "query", "search", "webmap" })]
    [ArcGIS.Samples.Shared.Attributes.ClassFile("Helpers/ArcGISLoginPrompt.cs")]
    public partial class SearchPortalMaps : ContentPage
    {
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        public SearchPortalMaps()
        {
            InitializeComponent();

            _ = Initialize();
        }

        private async Task Initialize()
        {
            ArcGISLoginPrompt.SetChallengeHandler();

            bool loggedIn = await ArcGISLoginPrompt.EnsureAGOLCredentialAsync();

            // Display a default map
            if (loggedIn) DisplayDefaultMap();
        }

        private void DisplayDefaultMap() => MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);

        private async Task SearchPublicMaps(string searchText)
        {
            try
            {
                // Get web map portal items from a keyword search
                IEnumerable<PortalItem> mapItems;

                // Connect to the portal (anonymously)
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(ArcGISOnlineUrl));

                // Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags
                string queryExpression = $"tags:\"{searchText}\" access:public type: (\"web map\" NOT \"web mapping application\")";

                // Create a query parameters object with the expression and a limit of 10 results
                PortalQueryParameters queryParams = new PortalQueryParameters(queryExpression, 10);

                // Search the portal using the query parameters and await the results
                PortalQueryResultSet<PortalItem> findResult = await portal.FindItemsAsync(queryParams);

                // Get the items from the query results
                mapItems = findResult.Results;

                // Hide the search controls
                SearchMapsUI.IsVisible = false;

                // Show the list of web maps
                MapsListView.ItemsSource = mapItems.ToList(); // Explicit ToList() needed to avoid Maui ListView bug.
                MapsListBorder.IsVisible = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void ShowSearchUI(object sender, EventArgs e)
        {
            // Show the map search controls
            MapsListBorder.IsVisible = false;
            SearchMapsUI.IsVisible = true;
        }

        private async void GetMyMaps(object sender, EventArgs e)
        {
            try
            {
                // Call a sub that will force the user to log in to ArcGIS Online (if they haven't already)
                bool loggedIn = await ArcGISLoginPrompt.EnsureAGOLCredentialAsync();
                if (!loggedIn) { return; }

                // Connect to the portal (will connect using the provided credentials)
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(ArcGISOnlineUrl));

                // Get the user's content (items in the root folder and a collection of sub-folders)
                PortalUserContent myContent = await portal.User.GetContentAsync();

                // Get the web map items in the root folder
                IEnumerable<PortalItem> mapItems = from item in myContent.Items where item.Type == PortalItemType.WebMap select item;

                // Loop through all sub-folders and get web map items, add them to the mapItems collection
                foreach (PortalFolder folder in myContent.Folders)
                {
                    IEnumerable<PortalItem> folderItems = await portal.User.GetContentAsync(folder.FolderId);
                    mapItems = mapItems.Concat(from item in folderItems where item.Type == PortalItemType.WebMap select item);
                }

                // Show the list of web maps
                MapsListView.ItemsSource = mapItems.ToList(); // Explicit ToList() needed to avoid Maui ListView bug.
                MapsListBorder.IsVisible = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void MapItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Get the selected web map item in the list box
            PortalItem selectedMap = MapsListView.SelectedItem as PortalItem;
            if (selectedMap == null) { return; }

            // Create a new map, pass the web map portal item to the constructor
            Map webMap = new Map(selectedMap);

            // Handle change in the load status (to report load errors)
            webMap.LoadStatusChanged += WebMapLoadStatusChanged;

            // Show the web map in the map view
            MyMapView.Map = webMap;

            // Hide the list of maps
            MapsListBorder.IsVisible = false;
        }

        private void WebMapLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Report errors if map failed to load
            if (e.Status == LoadStatus.FailedToLoad)
            {
                Map map = (Map)sender;
                Exception err = map.LoadError;
                if (err != null)
                {
                    Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => Application.Current.MainPage.DisplayAlert(err.Message, "Map Load Error", "OK"));
                }
            }
        }

        private void CancelSearchClicked(object sender, EventArgs e)
        {
            // Hide the search controls if the cancel button is clicked
            SearchMapsUI.IsVisible = false;
        }

        private void SearchMapsClicked(object sender, EventArgs e)
        {
            // Search ArcGIS Online maps with the text entered
            _ = SearchPublicMaps(SearchTextEntry.Text);
        }

        private void ListCloseClicked(object sender, EventArgs e)
        {
            MapsListBorder.IsVisible = false;
        }
    }
}