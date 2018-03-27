// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.ManageBookmarks
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Manage bookmarks",
        "Map",
        "This sample demonstrates how to access and add bookmarks to a map.",
        "")]
    public partial class ManageBookmarks : ContentPage
    {
        public ManageBookmarks()
        {
            InitializeComponent();

            Title = "Manage bookmarks";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new map with a base map
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Add the map to the mapview
            MyMapView.Map = myMap;

            // Create a set of predefined bookmarks; each one follows the pattern of:
            // ~ Initialize a viewpoint pointing to a latitude longitude
            // ~ Create a new bookmark
            // ~ Give the bookmark a name
            // ~ Assign the viewpoint
            // ~ Add the bookmark to bookmark collection of the map
            // ~ Add the bookmark name to the UI combo box for the user to choose from

            // Bookmark-1
            Viewpoint myViewpoint1 = new Viewpoint(27.3805833, 33.6321389, 6000);
            Bookmark myBookmark1 = new Bookmark();
            myBookmark1.Name = "Mysterious Desert Pattern";
            myBookmark1.Viewpoint = myViewpoint1;
            MyMapView.Map.Bookmarks.Add(myBookmark1);
            bookmarkPicker.Items.Add(myBookmark1.Name);

            // Bookmark-2
            Viewpoint myViewpoint2 = new Viewpoint(37.401573, -116.867808, 6000);
            Bookmark myBookmark2 = new Bookmark();
            myBookmark2.Name = "Strange Symbol";
            myBookmark2.Viewpoint = myViewpoint2;
            MyMapView.Map.Bookmarks.Add(myBookmark2);
            bookmarkPicker.Items.Add(myBookmark2.Name);

            // Bookmark-3
            Viewpoint myViewpoint3 = new Viewpoint(-33.867886, -63.985, 40000);
            Bookmark myBookmark3 = new Bookmark();
            myBookmark3.Name = "Guitar-Shaped Forest";
            myBookmark3.Viewpoint = myViewpoint3;
            MyMapView.Map.Bookmarks.Add(myBookmark3);
            bookmarkPicker.Items.Add(myBookmark3.Name);

            // Bookmark-4
            Viewpoint myViewpoint4 = new Viewpoint(44.525049, -110.83819, 6000);
            Bookmark myBookmark4 = new Bookmark();
            myBookmark4.Name = "Grand Prismatic Spring";
            myBookmark4.Viewpoint = myViewpoint4;
            MyMapView.Map.Bookmarks.Add(myBookmark4);
            bookmarkPicker.Items.Add(myBookmark4.Name);

            // Set the initial combo box selection to the last bookmark added
            bookmarkPicker.SelectedIndex = 3;

            // Zoom to the last bookmark
            myMap.InitialViewpoint = myMap.Bookmarks.Last().Viewpoint;
        }

        private void BookmarkPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected bookmarks name
            var selectedBookmarkName = bookmarkPicker.Items[bookmarkPicker.SelectedIndex];

            // Get the collection of bookmarks in the map
            BookmarkCollection myBookmarkCollection = MyMapView.Map.Bookmarks;

            // Loop through each bookmark
            foreach (var myBookmark in myBookmarkCollection)
            {
                // Get the bookmarks name
                var theBookmarkName = myBookmark.Name;

                // If the selected bookmarks name matches one in the bookmark collection
                // set that to be the maps viewpoint
                if (theBookmarkName == selectedBookmarkName.ToString())
                {
                    MyMapView.SetViewpoint(myBookmark.Viewpoint);
                }
            }
        }

        // Member (aka. global) variables for the dynamically created page allowing
        // the user to add additional bookmarks
        public Entry myEntryBookmarkName;

        public ContentPage bookmarkAddPage;

        private async void ButtonAddBookmark_Clicked(object sender, EventArgs e)
        {
            // Create root layout
            var layout = new StackLayout();

            // Label for the UI to let the user know to add a bookmark
            var myLabel = new Label();
            myLabel.Text = "Bookmark Name:";
            layout.Children.Add(myLabel);

            // Entry location for the user to enter the bookmark name
            myEntryBookmarkName = new Entry();
            layout.Children.Add(myEntryBookmarkName);

            // Button to accept the users bookmark name
            var okButton = new Button();
            okButton.Text = "OK";
            okButton.Clicked += OkButton_Clicked;
            layout.Children.Add(okButton);

            // Create internal page for the navigation page
            bookmarkAddPage = new ContentPage()
            {
                Content = layout,
                Title = "Add Bookmark"
            };

            // Navigate to the dynamically created user interaction page
            await Navigation.PushAsync(bookmarkAddPage);
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            // Get the name from the text field
            var name = myEntryBookmarkName.Text;

            // Exit if the name is empty
            if (string.IsNullOrEmpty(name))
                return;

            // Check to see if there is a bookmark with same name
            bool doesNameExist = MyMapView.Map.Bookmarks.Any(b => b.Name == name);
            if (doesNameExist)
                return;

            // Create a new bookmark
            Bookmark myBookmark = new Bookmark();
            myBookmark.Name = name;

            // Get the current viewpoint from map and assign it to bookmark
            myBookmark.Viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Add the bookmark to bookmark collection of the map
            MyMapView.Map.Bookmarks.Add(myBookmark);

            // Add the bookmark name to the list of choices in the picker
            bookmarkPicker.Items.Add(name);

            // Close the user interaction page to add a bookmark
            Navigation.RemovePage(bookmarkAddPage);
            bookmarkAddPage = null;
        }
    }
}