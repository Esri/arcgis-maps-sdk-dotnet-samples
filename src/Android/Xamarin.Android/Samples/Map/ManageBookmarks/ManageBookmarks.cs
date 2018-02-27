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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.ManageBookmarks
{
    [Activity(Label = "ManageBookmarks")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Manage bookmarks",
        "Map",
        "This sample demonstrates how to access and add bookmarks to a map.",
        "")]
    public class ManageBookmarks : Activity
    {
        // MapView for the app
        private MapView _myMapView = new MapView();

        // Dialog for entering the name of a new bookmark
        private AlertDialog _newBookmarkDialog = null;

        // Button to show available bookmarks (in a menu)
        Button _bookmarksButton;

        // Text input for the name of a new bookmark
        private EditText _bookmarkNameText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Manage bookmarks";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map with a World Imagery base map
            var myMap = new Map(Basemap.CreateImageryWithLabels());

            // Add the map to the MapView
            _myMapView.Map = myMap;

            // Create a set of predefined bookmarks; each one follows the pattern of:
            // ~ Initialize a viewpoint pointing to a latitude longitude
            // ~ Create a new bookmark
            // ~ Give the bookmark a name
            // ~ Assign the viewpoint
            // ~ Add the bookmark to bookmark collection of the map

            // Bookmark-1
            Viewpoint myViewpoint1 = new Viewpoint(27.3805833, 33.6321389, 6000);
            Bookmark myBookmark1 = new Bookmark();
            myBookmark1.Name = "Mysterious Desert Pattern";
            myBookmark1.Viewpoint = myViewpoint1;
            _myMapView.Map.Bookmarks.Add(myBookmark1);

            // Bookmark-2
            Viewpoint myViewpoint2 = new Viewpoint(37.401573, -116.867808, 6000);
            Bookmark myBookmark2 = new Bookmark();
            myBookmark2.Name = "Strange Symbol";
            myBookmark2.Viewpoint = myViewpoint2;
            _myMapView.Map.Bookmarks.Add(myBookmark2);

            // Bookmark-3
            Viewpoint myViewpoint3 = new Viewpoint(-33.867886, -63.985, 40000);
            Bookmark myBookmark3 = new Bookmark();
            myBookmark3.Name = "Guitar-Shaped Forest";
            myBookmark3.Viewpoint = myViewpoint3;
            _myMapView.Map.Bookmarks.Add(myBookmark3);

            // Bookmark-4
            Viewpoint myViewpoint4 = new Viewpoint(44.525049, -110.83819, 6000);
            Bookmark myBookmark4 = new Bookmark();
            myBookmark4.Name = "Grand Prismatic Spring";
            myBookmark4.Viewpoint = myViewpoint4;
            _myMapView.Map.Bookmarks.Add(myBookmark4);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show bookmarks
            _bookmarksButton = new Button(this);
            _bookmarksButton.Text = "Bookmarks";
            _bookmarksButton.Click += OnBookmarksClicked;

            // Add bookmarks button to the layout
            layout.AddView(_bookmarksButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void OnBookmarksClicked(object sender, EventArgs e)
        {
            // Create menu to show bookmarks
            var bookmarksMenu = new PopupMenu(this, _bookmarksButton);
            bookmarksMenu.MenuItemClick += OnBookmarksMenuItemClicked;

            // Create a menu option for each of the map's bookmarks
            foreach (Bookmark mark in _myMapView.Map.Bookmarks)
            {
                bookmarksMenu.Menu.Add(mark.Name);
            }

            // Add a final menu item for adding a new bookmark for the current viewpoint
            bookmarksMenu.Menu.Add("Add ...");

            // Show menu in the view
            bookmarksMenu.Show();
        }

        private void OnBookmarksMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            var selectedBookmarkName = e.Item.TitleCondensedFormatted.ToString();

            // If this is the "Add ..." choice, call a function to create a new bookmark
            if (selectedBookmarkName == "Add ...")
            {
                AddBookmark();
                return;
            }

            // Get the collection of bookmarks in the map
            BookmarkCollection myBookmarkCollection = _myMapView.Map.Bookmarks;

            // Loop through bookmarks
            foreach (var myBookmark in myBookmarkCollection)
            {
                // Get this bookmark name
                var theBookmarkName = myBookmark.Name;

                // If this is the selected bookmark, use it to set the map's viewpoint
                if (theBookmarkName == selectedBookmarkName)
                {
                    _myMapView.SetViewpoint(myBookmark.Viewpoint);

                    // Set the name of the button to display the current bookmark
                    _bookmarksButton.Text = selectedBookmarkName;

                    break;
                }
            }
        }

        private void AddBookmark()
        {
            // Create a dialog for entering the bookmark name
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout
            LinearLayout dialogLayout = new LinearLayout(this);
            dialogLayout.Orientation = Orientation.Vertical;

            // Create a layout for the text entry
            LinearLayout nameTextLayout = new LinearLayout(this);
            nameTextLayout.Orientation = Orientation.Horizontal;

            // EditText control for entering the bookmark name
            _bookmarkNameText = new EditText(this);
            
            // Label for the text entry
            var nameLabel = new TextView(this);
            nameLabel.Text = "Name:";

            // Add the controls to the layout
            nameTextLayout.AddView(nameLabel);
            nameTextLayout.AddView(_bookmarkNameText);

            // Create a layout for the dialog buttons (OK and Cancel)
            LinearLayout buttonLayout = new LinearLayout(this);
            buttonLayout.Orientation = Orientation.Horizontal;

            // Button to cancel the new bookmark
            var cancelButton = new Button(this);
            cancelButton.Text = "Cancel";
            cancelButton.Click += (s, e) => _newBookmarkDialog.Dismiss();

            // Button to save the current viewpoint as a new bookmark
            var okButton = new Button(this);
            okButton.Text = "OK";
            okButton.Click += CreateNewBookmark;

            // Add the buttons to the layout
            buttonLayout.AddView(cancelButton);
            buttonLayout.AddView(okButton);

            // Build the dialog with the text control and button layouts
            dialogLayout.AddView(nameTextLayout);
            dialogLayout.AddView(buttonLayout);

            // Set dialog content
            dialogBuilder.SetView(dialogLayout);
            dialogBuilder.SetTitle("New Bookmark");

            // Show the dialog 
            _newBookmarkDialog = dialogBuilder.Show();
        }

        // Handler for the click event of the New Bookmark dialog's OK button
        private void CreateNewBookmark(object sender, EventArgs e)
        {
            if (_newBookmarkDialog != null)
            {
                // See if the bookmark name conflicts with an existing name
                var bookmarkName = _bookmarkNameText.Text.Trim();
                var nameExists = false;
                foreach (Bookmark bookmark in _myMapView.Map.Bookmarks)
                {
                    // See if this bookmark exists (or conflicts with the "Add ..." menu choice)
                    if (bookmarkName.ToLower() == bookmark.Name.ToLower() || bookmarkName.ToLower() == "add ...")
                    {
                        nameExists = true;
                        break;
                    }
                }

                // If the name is an empty string or exists in the collection, warn the user and return
                if (string.IsNullOrEmpty(bookmarkName) || nameExists)
                {
                    AlertDialog.Builder dlgBuilder = new AlertDialog.Builder(this);
                    dlgBuilder.SetTitle("Error");
                    dlgBuilder.SetMessage("Please enter a unique name for the bookmark.");
                    dlgBuilder.Show();
                    return;
                }

                // Create a new bookmark
                Bookmark newBookmark = new Bookmark();

                // Use the current viewpoint for the bookmark
                Viewpoint currentViewpoint = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                newBookmark.Viewpoint = currentViewpoint;

                // Set the name with the value entered by the user
                newBookmark.Name = bookmarkName;

                // Add the bookmark to the map's bookmark collection
                _myMapView.Map.Bookmarks.Add(newBookmark);

                // Show this bookmark name as the button text
                _bookmarksButton.Text = bookmarkName;

                // Dismiss the dialog for entering the bookmark name
                _newBookmarkDialog.Dismiss();
            }
        }
    }
}