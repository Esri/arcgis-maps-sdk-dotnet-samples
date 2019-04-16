// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.SymbolsFromMobileStyle
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
            "Read symbols from a mobile style",
            "Symbology",
            "Open a local mobile style file (.stylx) and read its contents.",
            "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1bd036f221f54a99abc9e46ff3511cbf")]
    public class SymbolsFromMobileStyle : Activity
    {
        private MapView _myMapView = new MapView();

        // UI control to show/hide symbol dialog (private scope so it can be enabled/disabled as needed).
        private Button _showHideSymbolButton;

        // Dialog for showing symbols.
        private SymbolDialogFragment _symbolDialog;

        private MultilayerPointSymbol _faceSymbol;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "Read symbols from a style";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new layout for the entire page
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a new layout for the toolbar (buttons)
            LinearLayout toolbar = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create a button to clear graphics from the map view.
            Button clearButton = new Button(this) { Text = "Clear" };
            clearButton.Click += (sender, e) =>
            {
                _myMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            };
            toolbar.AddView(clearButton);

            // Create a button to show or hide the symbol selection dialog, add it to the toolbar
            _showHideSymbolButton = new Button(this) { Text = "Choose symbol" };
            _showHideSymbolButton.Click += ShowSymbolSelector;
            _showHideSymbolButton.Enabled = true;
            toolbar.AddView(_showHideSymbolButton);

            // Add the toolbar to the layout
            layout.AddView(toolbar);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Create new Map with imagery basemap.
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView.
            _myMapView.Map = myMap;

            // Create overlay to display point graphics.
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add created overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(overlay);

            _myMapView.GeoViewTapped += GeoViewTapped;

            // Get the full path to the mobile style file (.stylx).
            string mobileStyleFilePath = DataManager.GetDataFolder("1bd036f221f54a99abc9e46ff3511cbf", "emoji-mobile.stylx");

            _symbolDialog = new SymbolDialogFragment(mobileStyleFilePath);
            _symbolDialog.OnSymbolComplete += (sender, e) => { _faceSymbol = e.SelectedPointSymbol; };
        }

        private void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            Graphic graphic = new Graphic(e.Location, _faceSymbol);
            _myMapView.GraphicsOverlays.First().Graphics.Add(graphic);
        }


        private void ShowSymbolSelector(object sender, EventArgs e)
        {
            // Show the directions dialog
            if (_symbolDialog != null)
            {
                _symbolDialog.Show(FragmentManager, "");
            }
        }
    }

    // A custom DialogFragment class to show symbol choices.
    public class SymbolDialogFragment : DialogFragment
    {
        private ListView _eyesList;
        private ListView _mouthList;
        private ListView _hatList;
        private ImageView _symbolPreviewImage;
        private SymbolStyle _emojiStyle;
        private readonly string _baseSymbolKey = "Face1";
        private Android.Graphics.Color _faceColor = Android.Graphics.Color.Yellow;
        private int _symbolSize = 20;
        private TextView _sizeLabel;
        private List<SymbolLayerInfo> _eyeSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _mouthSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _hatSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<string> _symbolKeys;
        // Raise an event so the listener can access input values when the form has been completed.
        public event EventHandler<OnSymbolCompleteEventArgs> OnSymbolComplete;

        public SymbolDialogFragment(string stylxFilepath)
        {
            ReadMobileStyle(stylxFilepath);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Dialog to display.
            LinearLayout dialogView = null;
            var x = _symbolKeys;
            // Get the context for creating the dialog controls.
            Android.Content.Context ctx = Activity.ApplicationContext;

            // Set a dialog title.
            Dialog.SetTitle("Choose symbol");

            try
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                // The container for the dialog is a vertical linear layout.
                dialogView = new LinearLayout(ctx)
                {
                    Orientation = Orientation.Vertical
                };
                dialogView.SetPadding(10, 0, 10, 10);

                // Create a list box for showing each category of symbol.
                // Eyes symbol list.
                _eyesList = new ListView(ctx)
                {
                    ChoiceMode = ChoiceMode.Single
                };

                SymbolListAdapter eyesAdapter = new SymbolListAdapter(Activity, _eyeSymbolInfos);
                eyesAdapter.OnSymbolSelectionChanged += async (sender, e) =>
                {
                    await UpdateSymbol();
                };
                _eyesList.Adapter = eyesAdapter;

                TextView eyesLabel = new TextView(ctx);
                eyesLabel.SetText("Eyes", TextView.BufferType.Normal);
                eyesLabel.Gravity = GravityFlags.Center;
                eyesLabel.TextSize = 15.0f;
                eyesLabel.SetTextColor(Android.Graphics.Color.Black);
                dialogView.AddView(eyesLabel);
                dialogView.AddView(_eyesList);

                // Mouth symbol list.
                _mouthList = new ListView(ctx)
                {
                    ChoiceMode = ChoiceMode.Single
                };
                SymbolListAdapter mouthAdapter = new SymbolListAdapter(Activity, _mouthSymbolInfos);
                mouthAdapter.OnSymbolSelectionChanged += async (sender, e) =>
                {
                    await UpdateSymbol();
                };
                _mouthList.Adapter = mouthAdapter;
                TextView mouthLabel = new TextView(ctx);
                mouthLabel.SetText("Mouth", TextView.BufferType.Normal);
                mouthLabel.Gravity = GravityFlags.Center;
                mouthLabel.TextSize = 15.0f;
                mouthLabel.SetTextColor(Android.Graphics.Color.Black);
                dialogView.AddView(mouthLabel);
                dialogView.AddView(_mouthList);

                // Hat symbol list.
                _hatList = new ListView(ctx);
                SymbolListAdapter hatAdapter = new SymbolListAdapter(Activity, _hatSymbolInfos);
                hatAdapter.OnSymbolSelectionChanged += async (sender, e) =>
                {
                    await UpdateSymbol();
                };
                _hatList.Adapter = hatAdapter;
                TextView hatLabel = new TextView(ctx); 
                hatLabel.SetText("Hat", TextView.BufferType.Normal);
                hatLabel.Gravity = GravityFlags.Center;
                hatLabel.TextSize = 15.0f;
                hatLabel.SetTextColor(Android.Graphics.Color.Black);
                dialogView.AddView(hatLabel);
                dialogView.AddView(_hatList);

                // Add a preview image for the symbol.
                _symbolPreviewImage = new ImageView(ctx);
                dialogView.AddView(_symbolPreviewImage);

                // Create buttons to set the symbol color.
                Button yellowButton = new Button(ctx);
                yellowButton.SetBackgroundColor(Android.Graphics.Color.Yellow);
                yellowButton.Click += async(sender, e) => { _faceColor = Android.Graphics.Color.Yellow; await UpdateSymbol(); };

                Button greenButton = new Button(ctx);
                greenButton.SetBackgroundColor(Android.Graphics.Color.Green);
                greenButton.Click += async(sender, e) => { _faceColor = Android.Graphics.Color.Green; await UpdateSymbol(); };

                Button pinkButton = new Button(ctx);
                pinkButton.SetBackgroundColor(Android.Graphics.Color.Pink);
                pinkButton.Click += async(sender, e) => { _faceColor = Android.Graphics.Color.Pink; await UpdateSymbol(); };

                // Add the color selection buttons to a horizontal layout.
                LinearLayout colorLayout = new LinearLayout(ctx)
                {
                    Orientation = Orientation.Horizontal
                };
                colorLayout.SetPadding(10, 20, 10, 20);
                colorLayout.AddView(yellowButton);
                colorLayout.AddView(greenButton);
                colorLayout.AddView(pinkButton);

                dialogView.AddView(colorLayout);

                // Create a slider (SeekBar) to adjust the symbol size.
                SeekBar sizeSlider = new SeekBar(ctx)
                {
                    // Set a maximum slider value of 60 and a current value of 20.
                    Max = 60,
                    Progress = 20
                };

                // When the slider value (Progress) changes, update the symbol with the new size.
                sizeSlider.ProgressChanged += async(s, e) =>
                {
                    _symbolSize = e.Progress;
                    _sizeLabel.Text = $"Size:{_symbolSize:0}";
                    await UpdateSymbol();
                };

                // Create a layout to show the slider and label.
                LinearLayout sliderLayout = new LinearLayout(ctx)
                {
                    Orientation = Orientation.Horizontal
                };
                sliderLayout.SetPadding(10, 10, 10, 10);
                sizeSlider.LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent,
                    1.0f
                );
                _sizeLabel = new TextView(ctx)
                {
                    Gravity = GravityFlags.Right,
                    TextSize = 10.0f,
                    Text = $"Size:{_symbolSize:0}"
                };
                _sizeLabel.SetTextColor(Android.Graphics.Color.Black);
                sliderLayout.AddView(sizeSlider);
                sliderLayout.AddView(_sizeLabel);

                dialogView.AddView(sliderLayout);

                // Add a button to select the symbol.
                Button selectSymbolButton = new Button(ctx)
                {
                    Text = "Select"
                };

                selectSymbolButton.Click += SelectSymbolButtonClick;
                dialogView.AddView(selectSymbolButton);

            }
            catch (Exception ex)
            {
                // Show the exception message .
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }

            UpdateSymbol();

            // Return the new view for display.
            return dialogView;
        }

        private async Task ReadMobileStyle(string mobileStyleFilePath)
        {
            // Open a mobile style file.
            _emojiStyle = await Esri.ArcGISRuntime.Symbology.SymbolStyle.OpenAsync(mobileStyleFilePath);

            // Get the default style search parameters.
            SymbolStyleSearchParameters searchParams = await _emojiStyle.GetDefaultSearchParametersAsync();

            // Search the style with the default parameters to return all symbol results.
            IList<SymbolStyleSearchResult> styleResults = await _emojiStyle.SearchSymbolsAsync(searchParams);


            // Loop through the results and put symbols into the appropriate list according to category.
            foreach (SymbolStyleSearchResult result in styleResults)
            {
                MultilayerPointSymbol multiLayerSym = result.Symbol as MultilayerPointSymbol;
                RuntimeImage swatch = await multiLayerSym.CreateSwatchAsync();
                Bitmap symbolImage = await swatch.ToImageSourceAsync();

                switch (result.Category)
                {
                    case "Eyes":
                        {
                            _eyeSymbolInfos.Add(new SymbolLayerInfo(result.Name, symbolImage, result.Key));
                            break;
                        }
                    case "Mouth":
                        {
                            _mouthSymbolInfos.Add(new SymbolLayerInfo(result.Name, symbolImage, result.Key));
                            break;
                        }
                    case "Hat":
                        {
                            _hatSymbolInfos.Add(new SymbolLayerInfo(result.Name, symbolImage, result.Key));
                            break;
                        }
                    case "Face":
                        {
                           // _symbolPreviewImage.SetImageBitmap(symbolImage);
                            break;
                        }
                }
            }
        }

        private async Task<MultilayerPointSymbol> GetCurrentSymbol()
        {
            if (_emojiStyle == null) { return null; }

            _symbolKeys = new List<string>
            {
                _baseSymbolKey
            };

            SymbolListAdapter eyesAdapter = _eyesList.Adapter as SymbolListAdapter;

            if (eyesAdapter.SelectedSymbolInfo != null) 
            {
                SymbolLayerInfo selectedEyesSymbol = eyesAdapter.SelectedSymbolInfo; 
                if (!string.IsNullOrEmpty(selectedEyesSymbol.Key))
                {
                    _symbolKeys.Add(selectedEyesSymbol.Key);
                }
            }

            SymbolListAdapter mouthAdapter = _mouthList.Adapter as SymbolListAdapter;

            if (mouthAdapter.SelectedSymbolInfo != null)
            {
                SymbolLayerInfo selectedMouthSymbol = mouthAdapter.SelectedSymbolInfo;
                if (!string.IsNullOrEmpty(selectedMouthSymbol.Key))
                {
                    _symbolKeys.Add(selectedMouthSymbol.Key);
                }
            }

            SymbolListAdapter hatAdapter = _hatList.Adapter as SymbolListAdapter;

            if (hatAdapter.SelectedSymbolInfo != null)
            {
                SymbolLayerInfo selectedHatSymbol = hatAdapter.SelectedSymbolInfo;
                if (!string.IsNullOrEmpty(selectedHatSymbol.Key))
                {
                    _symbolKeys.Add(selectedHatSymbol.Key);
                }
            }

            MultilayerPointSymbol faceSymbol = await _emojiStyle.GetSymbolAsync(_symbolKeys) as MultilayerPointSymbol;

            // Loop through all symbol layers and lock the color.
            foreach (SymbolLayer lyr in faceSymbol.SymbolLayers)
            {
                // Changing the color of the symbol will not affect this layer.
                lyr.IsColorLocked = true;
            }

            // Unlock the color for the base (first) layer. Changing the symbol color will change this layer's color.
            faceSymbol.SymbolLayers.First().IsColorLocked = false;

            // Set the symbol color using the last selected color.
            System.Drawing.Color symbolColor = System.Drawing.Color.FromArgb(_faceColor.A, _faceColor.R, _faceColor.G, _faceColor.B);
            faceSymbol.Color = symbolColor;

            // Set the symbol size from the slider.
            faceSymbol.Size = _symbolSize;

            return faceSymbol;
        }

        private async Task UpdateSymbol()
        {
            MultilayerPointSymbol emojiSymbol = await GetCurrentSymbol();
            if (emojiSymbol != null)
            {
                RuntimeImage swatch = await emojiSymbol.CreateSwatchAsync(100, 100, 96, System.Drawing.Color.White);
                Bitmap symbolImage = await swatch.ToImageSourceAsync();
                _symbolPreviewImage.SetImageBitmap(symbolImage);
            }
        }

        // A click handler for the choose symbol button.
        private async void SelectSymbolButtonClick(object sender, EventArgs e)
        {
            try
            {
                MultilayerPointSymbol sym = await GetCurrentSymbol();

                // Create a new OnSymbolCompleteEventArgs object to store the selected symbol.
                OnSymbolCompleteEventArgs symbolSelectedArgs = new OnSymbolCompleteEventArgs(sym);

                // Raise the OnSaveClicked event so the main activity can handle the event and use the symbol.
                OnSymbolComplete?.Invoke(this, symbolSelectedArgs);

                // Close the dialog
                Dismiss();
            }
            catch (Exception ex)
            {
                // Show the exception message (dialog will stay open so user can try again)
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }
    }

    public class OnSymbolCompleteEventArgs
    {
        public MultilayerPointSymbol SelectedPointSymbol { get; private set; }

        public OnSymbolCompleteEventArgs(MultilayerPointSymbol symbol)
        {
            SelectedPointSymbol = symbol;
        }
    }

    // A class that defines a custom list adapter to show symbol layers.
    public class SymbolListAdapter : BaseAdapter<SymbolLayerInfo>
    {
        // Store the current activity.
        private Activity _ctx;

        // List of symbol layer infos.
        private List<SymbolLayerInfo> _symbolLayerInfos;

        public SymbolLayerInfo SelectedSymbolInfo { get; set; }

        public event EventHandler<EventArgs> OnSymbolSelectionChanged;

        // Constructor that takes the current activity and a list of symbol infos.
        public SymbolListAdapter(Activity context, List<SymbolLayerInfo> symbolInfos) : base()
        {
            _ctx = context;
            _symbolLayerInfos = symbolInfos;
        }
        
        // Get the symbol info at the specified position.
        public override SymbolLayerInfo this[int position]
        {
            get
            {
                return _symbolLayerInfos.ElementAt(position);
            }
        }

        // Return the count of symbol infos in the list.
        public override int Count
        {
            get
            {
                return _symbolLayerInfos.Count;
            }
        }

        // Get an ID for the item at the specified position.
        public override long GetItemId(int position)
        {
            return position;
        }

        // Create a view to display an item in the list (SymbolLayerInfo).
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Re-use an existing view, if one is supplied (otherwise create a new one).
           // View cellView = convertView;
            //if (cellView == null)
            //{
                // Create a list item that shows one symbol swatch image.
                View cellView = _ctx.LayoutInflater.Inflate(Android.Resource.Layout.ActivityListItem, null);
            //}

            cellView.SetMinimumHeight(30);
            // Set the image with the swatch image.
            if (_symbolLayerInfos.ElementAt(position).Image != null)
            {
                cellView.FindViewById<ImageView>(Android.Resource.Id.Icon).SetImageBitmap(_symbolLayerInfos.ElementAt(position).Image);
            }
            else
            {
                cellView.FindViewById<TextView>(Android.Resource.Id.Text1).SetText("None", TextView.BufferType.Normal);
            }
            cellView.Click += (sender, e) => 
            { 
                SelectedSymbolInfo = _symbolLayerInfos.ElementAt(position);
                OnSymbolSelectionChanged?.Invoke(this, new EventArgs());
            };

            return cellView;
        }
    }

    public class SymbolLayerInfo
    {
        public Bitmap Image { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

        public SymbolLayerInfo(string name, Bitmap image, string key)
        {
            Name = name;
            Image = image;
            Key = key;
        }
    }
}