// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.Samples.ManageBookmarks
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Manage bookmarks",
        category: "Map",
        description: "Access and create bookmarks on a map.",
        instructions: "The map in the sample comes pre-populated with a set of bookmarks. To access a bookmark and move to that location, tap on a bookmark's name from the list. To add a bookmark, pan and/or zoom to a new location and tap on the 'Add Bookmark' button. Enter a unique name for the bookmark and tap ok, and the bookmark will be added to the list",
        tags: new[] { "bookmark", "extent", "location", "zoom" })]
    public partial class ManageBookmarks : ContentPage
    {
        public ManageBookmarks()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create new map with a base map
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery);

            BookmarkPicker.ItemsSource = MyMapView.Map.Bookmarks;

            // Create a set of predefined bookmarks; each one follows the pattern of:
            // ~ Initialize a viewpoint pointing to a latitude longitude
            // ~ Create a new bookmark
            // ~ Give the bookmark a name
            // ~ Assign the viewpoint
            // ~ Add the bookmark to bookmark collection of the map
            // ~ Add the bookmark name to the UI combo box for the user to choose from

            // Bookmark-1
            Viewpoint myViewpoint1 = new Viewpoint(27.3805833, 33.6321389, 6000);
            Bookmark myBookmark1 = new Bookmark
            {
                Name = "Mysterious Desert Pattern",
                Viewpoint = myViewpoint1
            };
            MyMapView.Map.Bookmarks.Add(myBookmark1);

            // Bookmark-2
            Viewpoint myViewpoint2 = new Viewpoint(-39.299987, 174.060858, 600000);
            Bookmark myBookmark2 = new Bookmark
            {
                Name = "Dormant Volcano",
                Viewpoint = myViewpoint2
            };
            MyMapView.Map.Bookmarks.Add(myBookmark2);

            // Bookmark-3
            Viewpoint myViewpoint3 = new Viewpoint(-33.867886, -63.985, 40000);
            Bookmark myBookmark3 = new Bookmark
            {
                Name = "Guitar-Shaped Forest",
                Viewpoint = myViewpoint3
            };
            MyMapView.Map.Bookmarks.Add(myBookmark3);

            // Bookmark-4
            Viewpoint myViewpoint4 = new Viewpoint(44.525049, -110.83819, 6000);
            Bookmark myBookmark4 = new Bookmark
            {
                Name = "Grand Prismatic Spring",
                Viewpoint = myViewpoint4
            };
            MyMapView.Map.Bookmarks.Add(myBookmark4);

            // Set the initial combo box selection to the last bookmark added
            BookmarkPicker.SelectedIndex = 0;
        }

        private void BookmarkPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Verify that the selected item is a bookmark.
            if(BookmarkPicker.SelectedItem is Bookmark bookmark)
            { 
                MyMapView.SetViewpoint(bookmark.Viewpoint);
            }
        }

        private async void ButtonAddBookmark_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Prompt the user for the new bookmark name.
                string name = await Application.Current.MainPage.DisplayPromptAsync("New bookmark", "Enter name for new bookmark");

                // Exit if the name is empty
                if (string.IsNullOrEmpty(name))
                    return;

                // Check to see if there is a bookmark with same name
                bool doesNameExist = MyMapView.Map.Bookmarks.Any(b => b.Name == name);
                if (doesNameExist)
                    return;

                // Create a new bookmark
                Bookmark myBookmark = new Bookmark
                {
                    Name = name,

                    // Get the current viewpoint from map and assign it to bookmark
                    Viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)
                };

                // Add the bookmark to bookmark collection of the map
                MyMapView.Map.Bookmarks.Add(myBookmark);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}