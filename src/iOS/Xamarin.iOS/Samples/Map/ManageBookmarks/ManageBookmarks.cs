// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ManageBookmarks
{
    [Register("ManageBookmarks")]
    public class ManageBookmarks : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public ManageBookmarks()
        {
            Title = "Manage bookmarks";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Add default bookmarks
            AddDefaultBookmarks();

            // Zoom to the last bookmark
            myMap.InitialViewpoint = myMap.Bookmarks.Last().Viewpoint;
        }

        //Add some default bookmarks
        private void AddDefaultBookmarks()
        {
            Viewpoint vp;
            Bookmark myBookmark;

            // Bookmark-1
            // Initialize a viewpoint pointing to a latitude longitude
            vp = new Viewpoint(27.3805833, 33.6321389, 6000);
            // Create a new bookmark
            myBookmark = new Bookmark();
            myBookmark.Name = "Mysterious Desert Pattern";
            // Assign the viewpoint 
            myBookmark.Viewpoint = vp;
            // Add the bookmark to bookmark collection of the map
            _myMapView.Map.Bookmarks.Add(myBookmark);

            // Bookmark-2
            vp = new Viewpoint(37.401573, -116.867808, 6000);
            myBookmark = new Bookmark();
            myBookmark.Name = "Strange Symbol";
            myBookmark.Viewpoint = vp;
            _myMapView.Map.Bookmarks.Add(myBookmark);

            // Bookmark-3
            vp = new Viewpoint(-33.867886, -63.985, 40000);
            myBookmark = new Bookmark();
            myBookmark.Name = "Guitar-Shaped Forest";
            myBookmark.Viewpoint = vp;
            _myMapView.Map.Bookmarks.Add(myBookmark);

            // Bookmark-4
            vp = new Viewpoint(44.525049, -110.83819, 6000);
            myBookmark = new Bookmark();
            myBookmark.Name = "Grand Prismatic Spring";
            myBookmark.Viewpoint = vp;
            _myMapView.Map.Bookmarks.Add(myBookmark);
        }

        private void OnAddBookmarksButtonClicked(object sender, EventArgs e)
        {
            //Create Alert for bookmark name
            var textInputAlertController = UIAlertController.Create("Provide the bookmark name", 
                string.Empty, UIAlertControllerStyle.Alert);
            
            //Add Text Input
            textInputAlertController.AddTextField(textField => { });

            //Add Actions
            var cancelAction = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null);
            var okayAction = UIAlertAction.Create("Done", UIAlertActionStyle.Default, alertAction =>{
                // Get the name from the text field
                var name = textInputAlertController.TextFields[0].Text;

                // Exit if the name is empty
                if (string.IsNullOrEmpty(name))
                    return;

                // Check to see if there is a bookmark with same name
                bool doesNameExist = _myMapView.Map.Bookmarks.Any(b => b.Name == name);
                if (doesNameExist)
                    return;

                // Create a new bookmark
                Bookmark myBookmark = new Bookmark();
                myBookmark.Name = name;
                // Get the current viewpoint from map and assign it to bookmark 
                myBookmark.Viewpoint = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                // Add the bookmark to bookmark collection of the map
                _myMapView.Map.Bookmarks.Add(myBookmark);

            });

            textInputAlertController.AddAction(cancelAction);
            textInputAlertController.AddAction(okayAction);

            //Present Alert
            PresentViewController(textInputAlertController, true, null);
        }

        private void OnShowBookmarksButtonClicked(object sender, EventArgs e)
        {
            // Create a new Alert Controller
            UIAlertController actionAlert = UIAlertController.Create("Bookmarks", string.Empty,
                UIAlertControllerStyle.Alert);

            // Add Bookmarks as Actions
            foreach(Bookmark myBookmark in _myMapView.Map.Bookmarks)
            {
                actionAlert.AddAction(UIAlertAction.Create(myBookmark.Name, UIAlertActionStyle.Default, 
                    (action) =>{
                        _myMapView.SetViewpoint(myBookmark.Viewpoint);
                    }));
            }

            // Display the alert
            PresentViewController(actionAlert, true, null);
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView = new MapView()
            {
                Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height)
            };

            // Create a bookmark button to show existing bookmarks
            var showBookmarksButton = new UIBarButtonItem() { Title = "Bookmarks", Style = UIBarButtonItemStyle.Plain };
            showBookmarksButton.Clicked += OnShowBookmarksButtonClicked;

            // Create a button to add new bookmark
            var addBookmarkButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            addBookmarkButton.Clicked += OnAddBookmarksButtonClicked;

            // Add the buttons to the toolbar
            SetToolbarItems(new UIBarButtonItem[] {showBookmarksButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null),
                addBookmarkButton}, false);

            // Show the toolbar
            NavigationController.ToolbarHidden = false;

            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}