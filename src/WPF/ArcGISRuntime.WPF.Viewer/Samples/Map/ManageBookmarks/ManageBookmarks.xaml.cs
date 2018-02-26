// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System.Linq;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.ManageBookmarks
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Manage bookmarks",
        "Map",
        "This sample demonstrates how to access and add bookmarks to a map.",
        "")]
    public partial class ManageBookmarks
    {
        public ManageBookmarks()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new map with a base map
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Set the map view, map property to the base map
            MyMapView.Map = myMap;

            // Create a set of predefined bookmarks; each one follows the pattern of:
            // ~ Initialize a viewpoint pointing to a latitude longitude
            // ~ Create a new bookmark
            // ~ Give the bookmark a name
            // ~ Assign the viewpoint
            // ~ Add the bookmark to bookmark collection of the map
            // ~ Add the bookmark to the UI combo box for the user to choose from

            // Bookmark-1
            Viewpoint myViewpoint1 = new Viewpoint(27.3805833, 33.6321389, 6000);
            Bookmark myBookmark1 = new Bookmark();
            myBookmark1.Name = "Mysterious Desert Pattern";
            myBookmark1.Viewpoint = myViewpoint1;
            MyMapView.Map.Bookmarks.Add(myBookmark1);
            bookmarkChooser.Items.Add(myBookmark1);

            // Bookmark-2
            Viewpoint myViewpoint2 = new Viewpoint(37.401573, -116.867808, 6000);
            Bookmark myBookmark2 = new Bookmark();
            myBookmark2.Name = "Strange Symbol";
            myBookmark2.Viewpoint = myViewpoint2;
            MyMapView.Map.Bookmarks.Add(myBookmark2);
            bookmarkChooser.Items.Add(myBookmark2);

            // Bookmark-3
            Viewpoint myViewpoint3 = new Viewpoint(-33.867886, -63.985, 40000);
            Bookmark myBookmark3 = new Bookmark();
            myBookmark3.Name = "Guitar-Shaped Forest";
            myBookmark3.Viewpoint = myViewpoint3;
            MyMapView.Map.Bookmarks.Add(myBookmark3);
            bookmarkChooser.Items.Add(myBookmark3);

            // Bookmark-4
            Viewpoint myViewpoint4 = new Viewpoint(44.525049, -110.83819, 6000);
            Bookmark myBookmark4 = new Bookmark();
            myBookmark4.Name = "Grand Prismatic Spring";
            myBookmark4.Viewpoint = myViewpoint4;
            MyMapView.Map.Bookmarks.Add(myBookmark4);
            bookmarkChooser.Items.Add(myBookmark4);

            // Set the initial combo box selection to the last bookmark added
            bookmarkChooser.SelectedItem = MyMapView.Map.Bookmarks.Last();

            // Zoom to the last bookmark
            myMap.InitialViewpoint = myMap.Bookmarks.Last().Viewpoint;

            // Hide the controls for adding an additional bookmark
            BorderAddBookmark.Visibility = System.Windows.Visibility.Hidden;
        }

        private void OnBookmarkChooserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected bookmark and apply the view point to the map
            Bookmark selectedBookmark = e.AddedItems[0] as Bookmark;
            MyMapView.SetViewpoint(selectedBookmark.Viewpoint);
        }

        private void ButtonAddBookmark_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Show the controls to add a bookmark
            BorderAddBookmark.Visibility = System.Windows.Visibility.Visible;
            ButtonAddBookmark.Visibility = System.Windows.Visibility.Hidden;
            TextBoxBookmarkName.Text = "";
        }

        private void ButtonCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Hide the controls to add a bookmark
            BorderAddBookmark.Visibility = System.Windows.Visibility.Hidden;
            ButtonAddBookmark.Visibility = System.Windows.Visibility.Visible;
        }

        private void ButtonAddDone_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get the name from the text field
            var name = TextBoxBookmarkName.Text;

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

            // Add the bookmark to the list of choices in the combo box
            bookmarkChooser.Items.Insert(bookmarkChooser.Items.Count, myBookmark);

            // Set the newly added bookmark to be the one selected in the combo box
            bookmarkChooser.SelectedItem = myBookmark;

            // Hide the controls to add a bookmark
            BorderAddBookmark.Visibility = System.Windows.Visibility.Hidden;
            ButtonAddBookmark.Visibility = System.Windows.Visibility.Visible;
        }
    }
}