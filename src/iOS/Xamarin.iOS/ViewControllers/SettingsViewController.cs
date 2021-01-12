// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using CoreGraphics;
using Esri.ArcGISRuntime;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using WebKit;

namespace ArcGISRuntime
{
    public class SettingsViewController : UIViewController
    {
        // WebViews for about and license pages.
        private WKWebView _aboutView;
        private WKWebView _licensesView;

        // UI control to switch between pages.
        private UISegmentedControl _switcher;

        // UI items for download page.
        private UIStackView _downloadView;
        private UILabel _statusLabel;
        private UIToolbar _buttonToolbar;
        private UIBarButtonItem _downloadAllButton;
        private UIBarButtonItem _deleteAllButton;
        private UIBarButtonItem _cancelButton;
        private UITableView _downloadTable;
        private UIStackView _apiKeyView;
        private UIButton _apiKeyButton;

        // Cancellation token for downloading items.
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // List of samples that contain offline data.
        private List<SampleInfo> _samples = SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).ToList();

        // Directory for loading HTML locally.
        private string _contentDirectoryPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Content/");

        private const string _lightMarkdownFile = "github-markdown.css";
        private const string _darkMarkdownFile = "github-markdown-dark.css";

        private bool _darkMode = false;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            // Setup the switcher.
            NavigationItem.TitleView = _switcher;
            _switcher.ValueChanged += TabChanged;

            LoadHTML();
        }

        private void LoadHTML()
        {
            // Create the about page.
            var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
            var rtVersionString = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location).FileVersion;
            string aboutPath = Path.Combine(NSBundle.MainBundle.BundlePath, "about.md");
            string aboutContent = File.ReadAllText(aboutPath) + rtVersionString;
            string aboutHTML = MarkdownToHTML(aboutContent);

            _aboutView.LoadHtmlString(aboutHTML, new NSUrl(_contentDirectoryPath, true));

            // Create the licenses page.
            string licensePath = Path.Combine(NSBundle.MainBundle.BundlePath, "licenses.md");
            string licenseContent = File.ReadAllText(licensePath);
            string licenseHTML = MarkdownToHTML(licenseContent);

            _licensesView.LoadHtmlString(licenseHTML, new NSUrl(_contentDirectoryPath, true));
        }

        private void CheckDarkMode()
        {
            _darkMode = UIDevice.CurrentDevice.CheckSystemVersion(12, 0) && TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark;
        }

        private string MarkdownToHTML(string rawMarkdown)
        {
            CheckDarkMode();

            string markdownFile = _darkMode ? _darkMarkdownFile : _lightMarkdownFile;

            string markdownCSSPath = Path.Combine(NSBundle.MainBundle.BundlePath, $"SyntaxHighlighting/{markdownFile}");
            string parsedMarkdown = new MarkedNet.Marked().Parse(rawMarkdown);

            string markdowntHTML = "<!doctype html>" +
                "<head>" +
                "<link rel=\"stylesheet\" href=\"" +
                markdownCSSPath +
                "\" />" +
                "<meta name=\"viewport\" content=\"width=" +
                UIScreen.MainScreen.Bounds.Width.ToString() +
                ", shrink-to-fit=YES\">" +
                "</head>" +
                "<body class=\"markdown-body\">" +
                parsedMarkdown +
                "</body>";

            return markdowntHTML;
        }

        private async void DownloadAll(object sender, EventArgs e)
        {
            try
            {
                // Get a token from a new CancellationTokenSource()
                CancellationToken token = _cancellationTokenSource.Token;

                // Make a list of tasks for downloading all of the samples.
                HashSet<string> itemIds = new HashSet<string>();
                List<Task> downloadTasks = new List<Task>();

                foreach (SampleInfo sample in _samples)
                {
                    foreach (string itemId in sample.OfflineDataItems)
                    {
                        itemIds.Add(itemId);
                    }
                }

                // Enable the cancel button.
                _buttonToolbar.Items = new[] { _cancelButton };

                // Download every item.
                foreach (var item in itemIds)
                {
                    _statusLabel.Text = "Downloading item: " + item;
                    await DataManager.DownloadDataItem(item, token);
                }

                new UIAlertView(null, "All data downloaded", (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            catch (OperationCanceledException)
            {
                new UIAlertView(null, "Download all canceled", (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                new UIAlertView("Error", "Download  all canceled", (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                // Reset the token source.
                _cancellationTokenSource = new CancellationTokenSource();

                // Reset the UI.
                _buttonToolbar.Items = new[]
                {
                    _downloadAllButton,
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    _deleteAllButton,
                };
                _statusLabel.Text = "Ready";
            }
        }

        private void CancelDownloadAll(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void DeleteAll(object sender, EventArgs e)
        {
            try
            {
                _statusLabel.Text = "Deleting all...";
                string offlineDataPath = DataManager.GetDataFolder();

                // Delete the entire directory of offline data.
                Directory.Delete(offlineDataPath, true);

                new UIAlertView(null, "All data deleted", (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                new UIAlertView("Error", "Couldn't delete the offline data folder", (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _statusLabel.Text = "Ready";
            }
        }

        private void TabChanged(object sender, EventArgs e)
        {
            switch (_switcher.SelectedSegment)
            {
                case 0:
                    _aboutView.Hidden = false;
                    _apiKeyView.Hidden = _licensesView.Hidden = _downloadView.Hidden = _buttonToolbar.Hidden = true;
                    break;

                case 1:
                    _licensesView.Hidden = false;
                    _apiKeyView.Hidden = _aboutView.Hidden = _downloadView.Hidden = _buttonToolbar.Hidden = true;
                    break;

                case 2:
                    _downloadView.Hidden = _buttonToolbar.Hidden = false;
                    _apiKeyView.Hidden = _aboutView.Hidden = _licensesView.Hidden = true;
                    break;
                case 3:
                    _apiKeyView.Hidden = false;
                    _licensesView.Hidden = _aboutView.Hidden = _downloadView.Hidden = _buttonToolbar.Hidden = true;
                    break;
            }
        }

        private void OpenApiKeyPrompt(object sender, EventArgs e)
        {
            NavigationController.PushViewController(new ApiKeyPrompt(), true);
        }

        public override void LoadView()
        {
            // Create and configure the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            // Used to switch between the different views.
            _switcher = new UISegmentedControl(new string[] { "About", "Licenses", "Offline data", "API key" }) { SelectedSegment = 0 };

            // Displays the about.md in a web view.
            _aboutView = new WKWebView(new CGRect(), new WKWebViewConfiguration()) { BackgroundColor = UIColor.Clear, Opaque = false };
            _aboutView.TranslatesAutoresizingMaskIntoConstraints = false;
            _aboutView.NavigationDelegate = new BrowserLinksNavigationDelegate();

            // Displays the licenses.md in a web view.
            _licensesView = new WKWebView(new CGRect(), new WKWebViewConfiguration()) { Hidden = true, BackgroundColor = UIColor.Clear, Opaque = false };
            _licensesView.TranslatesAutoresizingMaskIntoConstraints = false;
            _licensesView.NavigationDelegate = new BrowserLinksNavigationDelegate();

            // View for managing offline data for samples.
            _downloadView = new UIStackView { BackgroundColor = ApplicationTheme.BackgroundColor, Hidden = true };
            _downloadView.Axis = UILayoutConstraintAxis.Vertical;
            _downloadView.LayoutMarginsRelativeArrangement = true;
            _downloadView.Alignment = UIStackViewAlignment.Fill;
            _downloadView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Label for download status.
            _statusLabel = new UILabel() { Text = "Ready", TextAlignment = UITextAlignment.Center };

            // Buttons for downloading or deleting all items.
            _buttonToolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false, Hidden = true };

            _downloadAllButton = new UIBarButtonItem() { Title = "Download all" };
            _deleteAllButton = new UIBarButtonItem() { Title = "Delete all" };
            _cancelButton = new UIBarButtonItem() { Title = "Cancel" };

            _buttonToolbar.Items = new[]
            {
                _downloadAllButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _deleteAllButton,
            };

            // Table of samples with downloadable items.
            _downloadTable = new UITableView();
            _downloadTable.Source = new SamplesTableSource(_samples, _statusLabel);
            _downloadTable.RowHeight = 50;
            _downloadTable.AllowsSelection = false;

            // Add the views to the download view.
            _downloadView.AddArrangedSubview(_statusLabel);
            _downloadView.AddArrangedSubview(_downloadTable);

            // Create the API key management elements.
            _apiKeyButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _apiKeyButton.SetTitle("Manage API key", UIControlState.Normal);
            _apiKeyButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _apiKeyView = new UIStackView() { Hidden = true };
            _apiKeyView.Axis = UILayoutConstraintAxis.Vertical;
            _apiKeyView.TranslatesAutoresizingMaskIntoConstraints = false;
            _apiKeyView.Distribution = UIStackViewDistribution.Fill;
            _apiKeyView.Alignment = UIStackViewAlignment.Top;
            _apiKeyView.Spacing = 5;
            _apiKeyView.LayoutMarginsRelativeArrangement = true;
            _apiKeyView.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 10);
            _apiKeyView.AddArrangedSubview(_apiKeyButton);

            // Add sub views to main view.
            View.AddSubviews(_aboutView, _licensesView, _downloadView, _buttonToolbar, _apiKeyView);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                 _aboutView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 _aboutView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _aboutView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _aboutView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _licensesView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 _licensesView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _licensesView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _licensesView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _downloadView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 _downloadView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _downloadView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _downloadView.BottomAnchor.ConstraintEqualTo(_buttonToolbar.TopAnchor),

                 _statusLabel.HeightAnchor.ConstraintEqualTo(40),

                 _buttonToolbar.TopAnchor.ConstraintEqualTo(_downloadView.BottomAnchor),
                 _buttonToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _buttonToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _buttonToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _apiKeyView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 _apiKeyView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _apiKeyView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _downloadAllButton.Clicked += DownloadAll;
            _deleteAllButton.Clicked += DeleteAll;
            _cancelButton.Clicked += CancelDownloadAll;
            _apiKeyButton.TouchUpInside += OpenApiKeyPrompt;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _downloadAllButton.Clicked -= DownloadAll;
            _deleteAllButton.Clicked -= DeleteAll;
            _cancelButton.Clicked -= CancelDownloadAll;
            _apiKeyButton.TouchUpInside -= OpenApiKeyPrompt;
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            // Reload the html pages when switching to and from dark mode.
            if (previousTraitCollection.UserInterfaceStyle != TraitCollection.UserInterfaceStyle) LoadHTML();
        }

        private class SamplesTableSource : UITableViewSource
        {
            private List<SampleInfo> _samples;

            // Images for button icons.
            private UIImage _globeImage = UIImage.FromBundle("GlobeIcon").ApplyTintColor(ApplicationTheme.ForegroundColor);
            private UIImage _downloadImage = UIImage.FromBundle("DownloadIcon").ApplyTintColor(ApplicationTheme.ForegroundColor);

            // Label for changing status text on downloads.
            private UILabel _statusLabel;

            public SamplesTableSource(List<SampleInfo> samples, UILabel label)
            {
                // Set up offline data.
                _samples = samples;
                _statusLabel = label;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                // If the cell represents an attachment, get an attachment cell.
                if (indexPath.Row < _samples.Count)
                {
                    // Gets a cell for the specified row.
                    UITableViewCell cell;

                    // Make the cell.
                    cell = new UITableViewCell(UITableViewCellStyle.Default, "SampleCell");
                    cell.TextLabel.Text = _samples[indexPath.Row].SampleName;

                    // Make the accessory view
                    UIView accessoryView = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

                    UIButton agolButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
                    agolButton.SetImage(_globeImage, UIControlState.Normal);
                    agolButton.TouchUpInside += (s, e) => OpenSampleInAGOL(indexPath);
                    CGRect agolFrame = new CGRect(0, 0, _globeImage.Size.Width, _globeImage.Size.Height);
                    agolButton.Frame = agolFrame;

                    UIButton dlButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
                    dlButton.SetImage(_downloadImage, UIControlState.Normal);
                    dlButton.TouchUpInside += (s, e) => DownloadSample(indexPath);
                    CGRect frame = new CGRect(0, 0, _downloadImage.Size.Width, _downloadImage.Size.Height);
                    dlButton.Frame = frame;

                    accessoryView.AddSubviews(agolButton, dlButton);

                    accessoryView.Frame = new CGRect(0, 0, _downloadImage.Size.Width * 2 + 20, _downloadImage.Size.Height);

                    NSLayoutConstraint.ActivateConstraints(new[]
                    {
                         agolButton.TopAnchor.ConstraintEqualTo(accessoryView.TopAnchor),
                         agolButton.LeadingAnchor.ConstraintEqualTo(accessoryView.LeadingAnchor),
                         agolButton.TrailingAnchor.ConstraintEqualTo(dlButton.LeadingAnchor),
                         agolButton.BottomAnchor.ConstraintEqualTo(accessoryView.BottomAnchor),

                         dlButton.TopAnchor.ConstraintEqualTo(accessoryView.TopAnchor),
                         dlButton.LeadingAnchor.ConstraintEqualTo(agolButton.TrailingAnchor),
                         dlButton.TrailingAnchor.ConstraintEqualTo(accessoryView.TrailingAnchor),
                         dlButton.BottomAnchor.ConstraintEqualTo(accessoryView.BottomAnchor),
                    });

                    cell.Accessory = UITableViewCellAccessory.Checkmark;
                    cell.AccessoryView = accessoryView;

                    return cell;
                }
                return null;
            }

            private void OpenSampleInAGOL(NSIndexPath indexPath)
            {
                SampleInfo sample = _samples[indexPath.Row];

                foreach (var offlineItem in sample.OfflineDataItems)
                {
                    string onlinePath = $"https://www.arcgis.com/home/item.html?id={offlineItem}";

                    if (UIApplication.SharedApplication.CanOpenUrl(new NSUrl(onlinePath)))
                    {
                        UIApplication.SharedApplication.OpenUrl(new NSUrl(onlinePath));
                    }
                }
            }

            private async void DownloadSample(NSIndexPath indexPath)
            {
                _statusLabel.Text = "Downloading sample data...";
                SampleInfo sample = _samples[indexPath.Row];

                try
                {
                    await DataManager.EnsureSampleDataPresent(sample);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    new UIAlertView("Error", "Download canceled", (IUIAlertViewDelegate)null, "OK", null).Show();
                }
                finally
                {
                    _statusLabel.Text = "Data downloaded: " + sample.SampleName;
                }
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new UITableViewRowAction[1];
                actions[0] = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive, "Delete", DeleteSample);
                return actions;
            }

            private void DeleteSample(UITableViewRowAction action, NSIndexPath indexPath)
            {
                try
                {
                    SampleInfo sample = _samples[indexPath.Row];

                    foreach (string offlineItemId in sample.OfflineDataItems)
                    {
                        string offlineDataPath = DataManager.GetDataFolder(offlineItemId);

                        Directory.Delete(offlineDataPath, true);
                    }
                    new UIAlertView(null, $"Offline data deleted for {sample.SampleName}", (IUIAlertViewDelegate)null, "OK", null).Show();
                }
                catch (DirectoryNotFoundException)
                {
                    new UIAlertView(null, "Data is not present.", (IUIAlertViewDelegate)null, "OK", null).Show();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    new UIAlertView("Error", "Couldn't delete offline data.", (IUIAlertViewDelegate)null, "OK", null).Show();
                }
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (section == 0)
                {
                    return _samples?.Count ?? 0;
                }
                return 0;
            }
        }
    }
}