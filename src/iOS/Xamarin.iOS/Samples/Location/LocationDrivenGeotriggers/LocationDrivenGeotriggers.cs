﻿// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Geotriggers;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.LocationDrivenGeotriggers
{
    [Register("LocationDrivenGeotriggers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Set up location-driven Geotriggers",
        category: "Location",
        description: "Create a notification every time a given location data source has entered and/or exited a set of features or graphics.",
        instructions: "Observe a virtual walking tour of the Santa Barbara Botanic Garden. Information about the user's current Garden Section, as well as information about nearby points of interest within 10 meters will display or be removed from the UI when the user enters or exits the buffer of each feature.",
        tags: new[] { "alert", "arcade", "fence", "geofence", "geotrigger", "location", "navigation", "notification", "notify", "routing", "trigger", "Featured" })]
    public class LocationDrivenGeotriggers : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;
        private UIBarButtonItem _playPauseButton;
        private UIBarButtonItem _sectionButton;
        private UILabel _statusLabel;
        private UIStackView _overlayView;
        private UILabel _nameLabel;
        private UILabel _description;
        private UIImageView _imageView;
        private UIButton _closeButton;

        private UILabel _pointsLabel;

        private string _tourPolylineJSON = "{\"paths\":[[[-119.709881177746,34.4570041646846],[-119.709875813328,34.4570152227745],[-119.709869107805,34.4570240692453],[-119.709859720074,34.4570351273326],[-119.709853014551,34.4570539260775],[-119.709847650133,34.4570760422426],[-119.709848991238,34.4570926293626],[-119.70985569676,34.4571103222869],[-119.709873131119,34.4571202745552],[-119.709889224373,34.4571302268223],[-119.709902635418,34.4571357558591],[-119.709910682045,34.4571600836165],[-119.709910682045,34.4571744591062],[-119.709902635418,34.4571833055602],[-119.709889224373,34.4571910462067],[-119.70988251885,34.4571965752394],[-119.70988251885,34.4572032100782],[-119.709889224373,34.4572175855605],[-119.709898612104,34.4572264320099],[-119.709912023149,34.4572341726524],[-119.709901294313,34.4572419132941],[-119.709895929895,34.4572507597409],[-119.709897271,34.4572596061868],[-119.709902635418,34.4572728758539],[-119.709902635418,34.4572828281028],[-119.70990934094,34.457294991961],[-119.709912023149,34.4573038384022],[-119.709886542164,34.4573115790375],[-119.709861061178,34.4573248486963],[-119.709843626819,34.4573414357669],[-119.709836921297,34.4573668692686],[-119.709843626819,34.4573934085666],[-119.709827533565,34.4574055724087],[-119.709791323744,34.4574188420525],[-119.709749749504,34.4574332174977],[-119.709709516369,34.4574431697275],[-119.709734997354,34.4574807670294],[-119.709748062646,34.4575248306656],[-119.709757450378,34.4575635337324],[-119.709770861423,34.457600025179],[-119.709785613572,34.4576387282109],[-119.70980573014,34.4576730080242],[-119.709815117871,34.4577117110223],[-119.709821823394,34.4577504140025],[-119.709821823394,34.4577869053674],[-119.709821823394,34.4578256083127],[-119.70981780008,34.4578609938471],[-119.709819141185,34.457906331541],[-119.70981460448,34.4579890675855],[-119.709818627793,34.4580675790658],[-119.70982667442,34.4581118108532],[-119.709832038838,34.4581471962662],[-119.709834721047,34.4581947453913],[-119.709836062152,34.4582323423548],[-119.709834721047,34.4582787856393],[-119.709805216748,34.4583429215611],[-119.709759619195,34.4584026342716],[-119.709700610597,34.4584612411497],[-119.709645400048,34.4585103926263],[-119.709566274882,34.4585457778704],[-119.709493855239,34.4585944325566],[-119.709458986522,34.458622077252],[-119.709424117805,34.4586198656767],[-119.709386566878,34.4586110193749],[-119.70935438037,34.4586110193749],[-119.709339628221,34.4586231830396],[-119.709324876071,34.4586585682359],[-119.709306100608,34.4586862129101],[-119.709269890786,34.4587171749343],[-119.709244409801,34.4587238096523],[-119.709229657651,34.4587293385835],[-119.709212223293,34.4587459253751],[-119.70919076562,34.4587945799446],[-119.709174672366,34.4588503400161],[-119.709157238008,34.4589288506865],[-119.709153214694,34.4589951976744],[-119.709155896903,34.4590449578807],[-119.70916394353,34.4590958238387],[-119.709186742307,34.4591323146156],[-119.709218928815,34.4591621706939],[-119.709237704278,34.4591831805204],[-119.709241727592,34.4592252001575],[-119.709238133851,34.459258658624],[-119.709219358388,34.4592796684262],[-119.709207288447,34.4592962551085],[-119.709208629552,34.4593084186733],[-119.709271661463,34.4593791884701],[-119.709310553494,34.4594267368937],[-119.709330670062,34.4594510639836],[-119.709353468838,34.4595008239182],[-119.70936285657,34.4595362087426],[-119.709423221989,34.4595943612845],[-119.709455408497,34.4596297460692],[-119.709487595005,34.459665130839],[-119.709507711573,34.4596817174446],[-119.709523804827,34.4596861405389],[-119.709557332439,34.4596894578594],[-119.709586836739,34.4596894578594],[-119.709593542261,34.4596772943501],[-119.709590860052,34.4596496500041],[-119.709572084589,34.4595777746615],[-119.709566720171,34.4595313320996],[-119.709578790112,34.4595136396883],[-119.709590860052,34.4594992646013],[-119.709627069874,34.4594882068404],[-119.709675349636,34.4595567649343],[-119.709735699339,34.4596197941001],[-119.709775932474,34.4596795058974],[-119.709802754564,34.4597126790997],[-119.709832258863,34.4597359003334],[-119.70986712758,34.4597171021923],[-119.70986980979,34.4596839289918],[-119.709865786476,34.4596308518435],[-119.709876515312,34.4595788804365],[-119.70988187973,34.4595346494263],[-119.709879197521,34.4594926299449],[-119.709852375431,34.4594539277723],[-119.709806777878,34.4593953215911],[-119.709767885847,34.4593212344729],[-119.709720947189,34.4592195028005],[-119.709708720088,34.4591478732967],[-119.709710061193,34.4591058536206],[-119.709707378984,34.459079314867],[-119.709652393699,34.4590262373344],[-119.709617524982,34.4589499383221],[-119.709626771268,34.4588695962162],[-119.709683097658,34.4588032491285],[-119.709730036316,34.4587391135603],[-119.709759540615,34.4586993052518],[-119.7097850216,34.4586650258598],[-119.710059948024,34.4587744987075],[-119.710104204473,34.4587932970608],[-119.710128443889,34.4587649224307],[-119.71019415801,34.4587096331253],[-119.710273283176,34.4586731421637],[-119.71031619852,34.4586532379961],[-119.710367160491,34.4586023717685],[-119.710392641477,34.4585747270665],[-119.710432874612,34.4585083797445],[-119.710471766643,34.4584453497398],[-119.710505294256,34.4584165991955],[-119.710575487456,34.4583689119728],[-119.710705574593,34.4583136224052],[-119.710780676446,34.4582627559707],[-119.710839685044,34.4582030431601],[-119.710895995717,34.4581546042213],[-119.710948298793,34.458095997128],[-119.710988531928,34.4580263320391],[-119.711015354018,34.4579511379096],[-119.711011330705,34.4579102234284],[-119.710985849719,34.4578847900921],[-119.710946957688,34.4578715205223],[-119.710779319625,34.4578847900921],[-119.71073908649,34.4578847900921],[-119.7107122644,34.4578604625458],[-119.71069751225,34.4578295002039],[-119.710674713473,34.4578095958352],[-119.710642526965,34.4577996436491],[-119.710614363771,34.457830606002],[-119.71057547174,34.4578571451526],[-119.710539261918,34.4578737321174],[-119.71049500547,34.4578858958895],[-119.710452090125,34.4578836842947],[-119.710413198095,34.4578748379149],[-119.710375647168,34.4578527219614],[-119.710336755138,34.4578350291944],[-119.710299204211,34.4578107016336],[-119.71027506433,34.4577885856631],[-119.710269605616,34.4577886418169],[-119.710240101317,34.4577510446536],[-119.710198527077,34.4576758502763],[-119.710154270628,34.4575984442288],[-119.710112696389,34.4575265671206],[-119.710044300059,34.4574381029023],[-119.709978842634,34.4573543166616],[-119.709966772693,34.4573410470074],[-119.709972137111,34.4573200367174],[-119.70997481932,34.4573023438375],[-119.709973478216,34.4572846509538],[-119.709970796007,34.457259217427],[-119.709931903976,34.4572348896984],[-119.709914469617,34.4572337838924],[-119.709893011945,34.4572171968005],[-119.709884965318,34.4571972922858],[-119.709902232039,34.4571821133624],[-119.70991161977,34.457167737874],[-119.709907596457,34.4571389868898],[-119.709919666397,34.4571235055865],[-119.709922348606,34.4571047068572],[-119.709918325293,34.4570836965077],[-119.709919666397,34.4570648977695],[-119.70992637192,34.4570516280694],[-119.709933077442,34.4570339351326],[-119.709935759651,34.4570151363832],[-119.709927713024,34.4570062899114],[-119.70991161977,34.4570018666751],[-119.709883456576,34.4570040782933]]],\"spatialReference\":{\"wkid\":4326,\"latestWkid\":4326}}";

        private SimulatedLocationDataSource _simulatedSource;
        private LocationGeotriggerFeed _geotriggerFeed;

        private ServiceFeatureTable _gardenSections;
        private ServiceFeatureTable _gardenPoints;

        private GeotriggerMonitor _sectionMonitor;
        private GeotriggerMonitor _pointsMonitor;

        private List<GeotriggerFeature> _features = new List<GeotriggerFeature>();

        private string _currentSection;
        private List<string> _pointsOfInterest = new List<string>();

        public LocationDrivenGeotriggers()
        {
            Title = "Set up location-driven LocationDrivenGeotriggers";
        }

        private async Task Initialize()
        {
            try
            {
                // This sample uses a web map with a predefined tile basemap, feature styles, and labels.
                _myMapView.Map = new Map(new Uri("https://www.arcgis.com/home/item.html?id=6ab0e91dc39e478cae4f408e1a36a308"));
                await _myMapView.Map.LoadAsync();

                // Get service feature tables from the map to create GeotriggerMonitors for.
                _gardenSections = ((FeatureLayer)_myMapView.Map.OperationalLayers[0]).FeatureTable as ServiceFeatureTable;
                _gardenPoints = ((FeatureLayer)_myMapView.Map.OperationalLayers[2]).FeatureTable as ServiceFeatureTable;

                // Create a simulated location data source for simulating a path through the data.
                _simulatedSource = new SimulatedLocationDataSource();

                // Create SimulationParameters starting at the current time, velocity, and horizontal and vertical accuracy.
                SimulationParameters simulationParameters = new SimulationParameters(DateTime.Now, 3.0, 0.0, 0.0);

                // Use the polyline as defined above or from this AGOL GeoJSON to define the path. Retrieved from https://https://arcgisruntime.maps.arcgis.com/home/item.html?id=2a346cf1668d4564b8413382ae98a956
                _simulatedSource.SetLocationsWithPolyline(Geometry.FromJson(_tourPolylineJSON) as Polyline, simulationParameters);

                // Set up the location display.
                _myMapView.LocationDisplay.DataSource = _simulatedSource;
                _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                _myMapView.LocationDisplay.InitialZoomScale = 1000;
                await _simulatedSource?.StartAsync();

                // LocationGeotriggerFeed will be used in instantiating a FenceGeotrigger.
                _geotriggerFeed = new LocationGeotriggerFeed(_simulatedSource);

                // Create monitors for sections and points of interest.
                _sectionMonitor = CreateGeotriggerMonitor(_gardenSections, 0.0, "Section Geotrigger");
                _pointsMonitor = CreateGeotriggerMonitor(_gardenPoints, 10.0, "POI Geotrigger");

                // Start both Geotrigger monitors.
                await _sectionMonitor?.StartAsync();
                await _pointsMonitor?.StartAsync();
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private GeotriggerMonitor CreateGeotriggerMonitor(ServiceFeatureTable table, double bufferSize, string triggerName)
        {
            // Create parameters for the fence.
            FeatureFenceParameters fenceParameters = new FeatureFenceParameters(table, bufferSize);

            // The ArcadeExpression defined in the following FenceGeotrigger returns the value for the "name" field of the feature that triggered the monitor.
            FenceGeotrigger fenceTrigger = new FenceGeotrigger(_geotriggerFeed, FenceRuleType.EnterOrExit, fenceParameters, new ArcadeExpression("$fenceFeature.name"), triggerName);

            // Create the monitor and set its event handler for notifications.
            GeotriggerMonitor geotriggerMonitor = new GeotriggerMonitor(fenceTrigger);
            geotriggerMonitor.Notification += HandleGeotriggerNotification;

            return geotriggerMonitor;
        }

        private void HandleGeotriggerNotification(object sender, GeotriggerNotificationInfo info)
        {
            // The collection used for the list view is changed, and must be modified on a UI thread.
            BeginInvokeOnMainThread(async () =>
           {
               if (info is FenceGeotriggerNotificationInfo fenceInfo)
               {
                   if (fenceInfo.FenceNotificationType == FenceNotificationType.Entered)
                   {
                       if (!_features.Any(f => f.Name == fenceInfo.Message))
                       {
                           try
                           {
                               // Get the feature that's fence has been entered.
                               ArcGISFeature feature = fenceInfo.FenceGeoElement as ArcGISFeature;

                               // Get the description for the feature.
                               string description = feature.Attributes["desc_raw"].ToString();

                               // Get the attachments for the feature.
                               IReadOnlyList<Attachment> attach = await feature.GetAttachmentsAsync();

                               // Load the attachment data into an image source.
                               Stream attachmentDataStream = await attach.First().GetDataAsync();
                               NSData imageData = NSData.FromStream(attachmentDataStream);
                               UIImage image = UIImage.LoadFromData(imageData);

                               // Create an object to group the information together for interface use.
                               GeotriggerFeature geotriggerFeature = new GeotriggerFeature
                               {
                                   Name = fenceInfo.Message,
                                   Description = description,
                                   ImageSource = image,
                               };

                               // Add the object to the list of known feature information.
                               _features.Add(geotriggerFeature);
                           }
                           catch (Exception ex)
                           {
                               new UIAlertView("Error", ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
                           }
                       }

                       if (fenceInfo.GeotriggerMonitor == _pointsMonitor)
                       {
                           _pointsOfInterest.Add(fenceInfo.Message);
                       }

                       if (fenceInfo.GeotriggerMonitor == _sectionMonitor)
                       {
                           _currentSection = fenceInfo.Message;
                       }

                       _statusLabel.Text = $"Entered {fenceInfo.Message} fence.";
                   }
                   else
                   {
                       if (fenceInfo.GeotriggerMonitor == _pointsMonitor)
                       {
                           _pointsOfInterest.Remove(fenceInfo.Message);
                       }
                       if (fenceInfo.GeotriggerMonitor == _sectionMonitor && _currentSection == fenceInfo.Message)
                       {
                           _currentSection = string.Empty;
                       }

                       _statusLabel.Text = $"Exited {fenceInfo.Message} fence.";
                   }
               }

               // Update the UI with the current section and points of interest.
               _sectionButton.Title = $"Section: {_currentSection}";

               if (_pointsOfInterest.Any())
               {
                   _pointsLabel.Text = $"Points of interest: {string.Join(", ", _pointsOfInterest)}";
               }
               else
               {
                   _pointsLabel.Text = $"  Points of interest:";
               }
           });
        }

        private void DisplayData(GeotriggerFeature feature)
        {
            // Update the UI with data about the selected feature.
            _nameLabel.Text = feature.Name;
            _description.Text = feature.Description;
            _imageView.Image = feature.ImageSource;
            _overlayView.Hidden = false;
        }

        private void PlayPauseButton_Pressed(object sender, EventArgs e)
        {
            // Start and stop the simulated location on a button press.
            if (_simulatedSource.Status == LocationDataSourceStatus.Started)
            {
                _simulatedSource.StopAsync();
                _playPauseButton.Title = "\uE23A";
            }
            else
            {
                _simulatedSource.StartAsync();
                _playPauseButton.Title = "\u23F8";
            }
        }

        private void SectionButton_Clicked(object sender, EventArgs e)
        {
            // Display data for the section the user is currently in.
            if (!string.IsNullOrEmpty(_currentSection))
            {
                DisplayData(_features.First(f => f.Name == _currentSection));
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _statusLabel = new UILabel
            {
                Text = "",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _pointsLabel = new UILabel
            {
                Text = "  Points of interest:",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Left,
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TextColor = ApplicationTheme.ForegroundColor,
                Lines = 1,
                DirectionalLayoutMargins = new NSDirectionalEdgeInsets(0, 10, 0, 10),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _sectionButton = new UIBarButtonItem();
            _sectionButton.Title = "Section:";

            _playPauseButton = new UIBarButtonItem();
            _playPauseButton.Title = "\u23F8";

            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            _toolbar.Items = new[]
            {
                _sectionButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _playPauseButton
            };

            _overlayView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Distribution = UIStackViewDistribution.EqualSpacing,
                Alignment = UIStackViewAlignment.Leading,
                Spacing = 5,
                LayoutMarginsRelativeArrangement = true,
                DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 0),
                Hidden = true,
                BackgroundColor = ApplicationTheme.BackgroundColor,
            };

            _nameLabel = new UILabel() { TranslatesAutoresizingMaskIntoConstraints = false, Lines = 0 };
            _overlayView.AddArrangedSubview(_nameLabel);

            _imageView = new UIImageView() { TranslatesAutoresizingMaskIntoConstraints = false, };
            _overlayView.AddArrangedSubview(_imageView);

            _description = new UILabel() { TranslatesAutoresizingMaskIntoConstraints = false, Lines = 15 };
            _overlayView.AddArrangedSubview(_description);

            _closeButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _closeButton.SetTitle("Close", UIControlState.Normal);
            _closeButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _overlayView.AddArrangedSubview(_closeButton);

            // Add the views.
            View.AddSubviews(_myMapView, _statusLabel, _pointsLabel, _toolbar, _overlayView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
                {
                    _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                    _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _myMapView.BottomAnchor.ConstraintEqualTo(_pointsLabel.TopAnchor),

                    _pointsLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _pointsLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _pointsLabel.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor),
                    _pointsLabel.HeightAnchor.ConstraintEqualTo(40),

                    _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                    _overlayView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor, 0.9f),
                    _overlayView.HeightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.HeightAnchor, 0.8f),
                    _overlayView.CenterXAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterXAnchor),
                    _overlayView.CenterYAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterYAnchor),

                    _statusLabel.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                    _statusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _statusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _statusLabel.HeightAnchor.ConstraintEqualTo(40),

                    _imageView.HeightAnchor.ConstraintEqualTo(165),
                    _imageView.WidthAnchor.ConstraintEqualTo(115),
                }
            );
        }

        private void CloseButton_Pressed(object sender, EventArgs e)
        {
            _overlayView.Hidden = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _ = Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _playPauseButton.Clicked += PlayPauseButton_Pressed;
            _sectionButton.Clicked += SectionButton_Clicked;
            _closeButton.TouchUpInside += CloseButton_Pressed;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _playPauseButton.Clicked -= PlayPauseButton_Pressed;
            _sectionButton.Clicked -= SectionButton_Clicked;
            _closeButton.TouchUpInside -= CloseButton_Pressed;

            // Dispose of images.
            foreach (UIImage image in _features.Select(f => f.ImageSource))
            {
                image?.Dispose();
            }

            // Stop the location data source.
            _myMapView?.LocationDisplay?.DataSource?.StopAsync();
        }
    }

    // Class to store attributes of features together.
    public class GeotriggerFeature
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public UIImage ImageSource { get; set; }
    }
}