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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ManageBookmarks
{
    [Register("ManageBookmarks")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Manage bookmarks",
        category: "Map",
        description: "Access and create bookmarks on a map.",
        instructions: "The map in the sample comes pre-populated with a set of bookmarks. To access a bookmark and move to that location, tap on a bookmark's name from the list. To add a bookmark, pan and/or zoom to a new location and tap on the 'Add Bookmark' button. Enter a unique name for the bookmark and tap ok, and the bookmark will be added to the list",
        tags: new[] { "bookmark", "extent", "location", "zoom" })]
    public class ManageBookmarks : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _bookmarksButton;
        private UIBarButtonItem _addButton;
        private UIAlertController _textInputAlertController;

        public ManageBookmarks()
        {
            Title = "Manage bookmarks";
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
            if (_textInputAlertController == null)
            {
                _textInputAlertController = UIAlertController.Create("Provide the bookmark name", "", UIAlertControllerStyle.Alert);

                // Add Text Input.
                _textInputAlertController.AddTextField(textField => { });

                void HandleAlertAction(UIAlertAction action)
                {
                    // Get the name from the text field.
                    string name = _textInputAlertController.TextFields[0].Text;

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
                }

                // Add Actions.
                _textInputAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                _textInputAlertController.AddAction(UIAlertAction.Create("Done", UIAlertActionStyle.Default, HandleAlertAction));
            }

            // Show the alert.
            PresentViewController(_textInputAlertController, true, null);
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _bookmarksButton = new UIBarButtonItem();
            _bookmarksButton.Title = "Bookmarks";

            _addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _bookmarksButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _addButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _bookmarksButton.Clicked += OnShowBookmarksButtonClicked;
            _addButton.Clicked += OnAddBookmarksButtonClicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _bookmarksButton.Clicked -= OnShowBookmarksButtonClicked;
            _addButton.Clicked -= OnAddBookmarksButtonClicked;

            // Remove the reference to the alert controller, preventing a memory leak.
            _textInputAlertController = null;
        }
    }
}