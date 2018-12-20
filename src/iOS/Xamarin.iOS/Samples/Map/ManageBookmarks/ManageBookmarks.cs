// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Linq;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ManageBookmarks
{
    [Register("ManageBookmarks")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Manage bookmarks",
        "Map",
        "This sample demonstrates how to access and add bookmarks to a map.",
        "")]
    public class ManageBookmarks : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;

        public ManageBookmarks()
        {
            Title = "Manage bookmarks";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with labeled imagery basemap.
            Map map = new Map(Basemap.CreateImageryWithLabels());

            // Add default bookmarks.
            AddDefaultBookmarks(map);

            // Zoom to the last bookmark.
            map.InitialViewpoint = map.Bookmarks.Last().Viewpoint;

            // Show the map.
            _myMapView.Map = map;
        }

        private void AddDefaultBookmarks(Map map)
        {
            // Bookmark 1.
            // Create a new bookmark.
            Bookmark myBookmark = new Bookmark
            {
                Name = "Mysterious Desert Pattern",
                Viewpoint = new Viewpoint(27.3805833, 33.6321389, 6000)
            };
            // Add the bookmark to bookmark collection of the map.
            map.Bookmarks.Add(myBookmark);

            // Bookmark 2.
            myBookmark = new Bookmark
            {
                Name = "Dormant Volcano",
                Viewpoint = new Viewpoint(-39.299987, 174.060858, 600000)
            };
            map.Bookmarks.Add(myBookmark);

            // Bookmark 3.
            myBookmark = new Bookmark
            {
                Name = "Guitar-Shaped Forest",
                Viewpoint = new Viewpoint(-33.867886, -63.985, 40000)
            };
            map.Bookmarks.Add(myBookmark);

            // Bookmark 4.
            myBookmark = new Bookmark
            {
                Name = "Grand Prismatic Spring",
                Viewpoint = new Viewpoint(44.525049, -110.83819, 6000)
            };
            map.Bookmarks.Add(myBookmark);
        }

        private void OnAddBookmarksButtonClicked(object sender, EventArgs e)
        {
            // Create Alert for naming the bookmark.
            var textInputAlertController = UIAlertController.Create("Provide the bookmark name", "", UIAlertControllerStyle.Alert);

            // Add Text Input.
            textInputAlertController.AddTextField(textField => { });

            // Add Actions.
            var cancelAction = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null);
            var okayAction = UIAlertAction.Create("Done", UIAlertActionStyle.Default, alertAction =>
            {
                // Get the name from the text field.
                string name = textInputAlertController.TextFields[0].Text;

                // Exit if the name is empty.
                if (String.IsNullOrEmpty(name))
                    return;

                // Check to see if there is a bookmark with same name.
                bool doesNameExist = _myMapView.Map.Bookmarks.Any(b => b.Name == name);
                if (doesNameExist)
                    return;

                // Create a new bookmark.
                Bookmark myBookmark = new Bookmark
                {
                    Name = name,
                    // Get the current viewpoint from map and assign it to bookmark .
                    Viewpoint = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)
                };

                // Add the bookmark to bookmark collection of the map.
                _myMapView.Map.Bookmarks.Add(myBookmark);
            });

            textInputAlertController.AddAction(cancelAction);
            textInputAlertController.AddAction(okayAction);

            // Show the alert.
            PresentViewController(textInputAlertController, true, null);
        }

        private void OnShowBookmarksButtonClicked(object sender, EventArgs e)
        {
            // Create a new Alert Controller.
            UIAlertController actionAlert = UIAlertController.Create("Bookmarks", "",
                UIAlertControllerStyle.Alert);

            // Add Bookmarks as Actions.
            foreach (Bookmark myBookmark in _myMapView.Map.Bookmarks)
            {
                actionAlert.AddAction(UIAlertAction.Create(myBookmark.Name, UIAlertActionStyle.Default, action => _myMapView.SetViewpoint(myBookmark.Viewpoint)));
            }

            // Display the alert.
            PresentViewController(actionAlert, true, null);
        }

        public override void LoadView()
        {
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            
            View.AddSubviews(_myMapView, _toolbar);

            _toolbar.Items = new[]
            {
                new UIBarButtonItem("Bookmarks", UIBarButtonItemStyle.Plain, OnShowBookmarksButtonClicked), 
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(UIBarButtonSystemItem.Add, OnAddBookmarksButtonClicked)
            };

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor).Active = true;

            _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }
    }
}