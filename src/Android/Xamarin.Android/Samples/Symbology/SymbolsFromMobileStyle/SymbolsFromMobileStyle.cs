// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.Samples.SymbolsFromMobileStyle
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Read symbols from mobile style",
        category: "Symbology",
        description: "Combine multiple symbols from a mobile style file into a single symbol.",
        instructions: "Select a symbol and a color from each of the category lists to create an emoji. A preview of the symbol is updated as selections are made. The size of the symbol can be set using the slider. Tap the map to create a point graphic using the customized emoji symbol, and tap \"Reset\" to clear all graphics from the display.",
        tags: new[] { "advanced symbology", "mobile style", "multilayer", "stylx" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1bd036f221f54a99abc9e46ff3511cbf")]
    public class SymbolsFromMobileStyle : Activity
    {
        // A map view to display a map and graphics.
        private MapView _myMapView;

        // A dialog for showing symbols from the stylx file.
        private SymbolDialogFragment _symbolDialog;

        // A multilayer symbol created by combining selected symbol layers from the style file.
        private MultilayerPointSymbol _faceSymbol;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "Read symbols from a style";

            // Create the UI.
            CreateLayout();

            // Initialize the app.
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the entire page.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a new horizontal layout for the toolbar (buttons).
            LinearLayout toolbar = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create a button to clear graphics from the map view, add it to the toolbar.
            Button clearButton = new Button(this) { Text = "Clear" };
            clearButton.Click += (sender, e) =>
            {
                _myMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            };
            toolbar.AddView(clearButton);

            // Create a button to show the symbol selection dialog, add it to the toolbar.
            Button showSymbolDialog = new Button(this) { Text = "Choose symbol" };
            toolbar.AddView(showSymbolDialog);

            // Handle the button click event with a function that shows the dialog.
            showSymbolDialog.Click += ShowSymbolSelector;

            // Add the toolbar to the layout.
            layout.AddView(toolbar);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Create new Map with a topographic basemap.
            Map myMap = new Map(Basemap.CreateTopographic());

            // Display the map in the map view.
            _myMapView.Map = myMap;

            // Create an overlay to display point graphics and add it to the map view.
            GraphicsOverlay overlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(overlay);

            // Handle the tap event on the map view to allow the user to place graphics.
            _myMapView.GeoViewTapped += GeoViewTapped;

            // Get the full path to the downloaded mobile style file (.stylx).
            string mobileStyleFilePath = DataManager.GetDataFolder("1bd036f221f54a99abc9e46ff3511cbf", "emoji-mobile.stylx");

            // Create the dialog for selecting symbol layers from the style.
            _symbolDialog = new SymbolDialogFragment(mobileStyleFilePath);

            // Handle the OnSymbolComplete event on the dialog to get the symbol the user created.
            _symbolDialog.OnSymbolComplete += (sender, e) => { _faceSymbol = e.SelectedPointSymbol; };
        }

        // Handler for the map view tapped event.
        private void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Create a new graphic using the tap location and the current face symbol.
            Graphic graphic = new Graphic(e.Location, _faceSymbol);

            // Add the graphic to the first (only) graphics overlay in the map view.
            _myMapView.GraphicsOverlays.First().Graphics.Add(graphic);
        }

        // A button click handler to show the symbol dialog.
        private void ShowSymbolSelector(object sender, EventArgs e)
        {
            _symbolDialog.Show(FragmentManager, "");
        }
    }

    // A custom DialogFragment class to show symbols from a mobile style file.
    public class SymbolDialogFragment : DialogFragment
    {
        // The mobile style that contains the symbols to display.
        private SymbolStyle _emojiStyle;

        // List view controls to show categories of symbols: eyes, mouth, hat.
        private ListView _eyesList;
        private ListView _mouthList;
        private ListView _hatList;

        // Lists of a custom type (SymbolLayerInfo) to contain symbol choices for each category (eyes, mouth, hat).
        private List<SymbolLayerInfo> _eyeSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _mouthSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _hatSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };

        // The currently selected symbol layer key for each category (the key uniquely identifies symbols in the style).
        private string _selectedEyesKey = "";
        private string _selectedMouthKey = "";
        private string _selectedHatKey = "";

        // The key that identifies the base symbol (circle representing the face).
        private const string _baseSymbolKey = "Face1";

        // A variable to store the selected color for the face (defaults to yellow).
        private Color _faceColor = Color.Yellow;

        // A variable to store the desired symbol size.
        private int _symbolSize = 20;

        // An image view to show a preview of the current symbol (the selected symbol layers combined into a multilayer symbol).
        private ImageView _symbolPreviewImage;

        // An event so the listener can access the final symbol when the dialog is dismissed.
        public event EventHandler<OnSymbolCompleteEventArgs> OnSymbolComplete;

        public SymbolDialogFragment(string stylxFilepath)
        {
            // When the dialog is created, use the path to the stylx file to open the mobile style and display the symbols.
            ReadMobileStyle(stylxFilepath);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Set a dialog title.
            Dialog.SetTitle("Define a symbol");

            // Get the context for creating the dialog controls.
            Android.Content.Context ctx = Activity.ApplicationContext;

            // The container for the dialog is a vertical linear layout.
            LinearLayout dialogView = new LinearLayout(ctx)
            {
                Orientation = Orientation.Vertical
            };
            dialogView.SetPadding(10, 0, 10, 10);

            try
            {
                // Create a list box for showing each category of symbol: eyes, mouth, hat.
                _eyesList = new ListView(ctx);
                _mouthList = new ListView(ctx);
                _hatList = new ListView(ctx);

                // Create a new list adapter to show the eye symbols in a list view.
                SymbolListAdapter eyesAdapter = new SymbolListAdapter(Activity, _eyeSymbolInfos);

                // Handle selection change events for the eyes symbol adapter.
                eyesAdapter.OnSymbolSelectionChanged += async (sender, e) =>
                {
                    // Get the key for the selected eyes (if any) then update the symbol.
                    if (eyesAdapter.SelectedSymbolInfo != null)
                    {
                        _selectedEyesKey = eyesAdapter.SelectedSymbolInfo?.Key;
                        await UpdateSymbol();
                    }

                };

                // Assign the adapter to the list view.
                _eyesList.Adapter = eyesAdapter;

                // Create a label to for the "eyes" symbol list.
                TextView eyesLabel = new TextView(ctx);
                eyesLabel.SetText("Eyes", TextView.BufferType.Normal);
                eyesLabel.Gravity = GravityFlags.Center;
                eyesLabel.TextSize = 15.0f;
                eyesLabel.SetTextColor(Android.Graphics.Color.Black);

                // Add the eyes label and list to the dialog.
                dialogView.AddView(eyesLabel);
                dialogView.AddView(_eyesList);

                // Create a new list adapter to show the mouth symbols in a list view.
                SymbolListAdapter mouthAdapter = new SymbolListAdapter(Activity, _mouthSymbolInfos);

                // Handle selection change events for the eyes symbol adapter.
                mouthAdapter.OnSymbolSelectionChanged += async (sender, e) =>
                {
                    // Get the key for the selected mouth (if any) then update the symbol.
                    if (mouthAdapter.SelectedSymbolInfo != null)
                    {
                        _selectedMouthKey = mouthAdapter.SelectedSymbolInfo?.Key;
                        await UpdateSymbol();
                    }
                };

                // Assign the adapter to the list view.
                _mouthList.Adapter = mouthAdapter;

                // Create a label to for the "mouth" symbol list.
                TextView mouthLabel = new TextView(ctx);
                mouthLabel.SetText("Mouth", TextView.BufferType.Normal);
                mouthLabel.Gravity = GravityFlags.Center;
                mouthLabel.TextSize = 15.0f;
                mouthLabel.SetTextColor(Android.Graphics.Color.Black);

                // Add the mouth label and list to the dialog.
                dialogView.AddView(mouthLabel);
                dialogView.AddView(_mouthList);

                // Create a new list adapter to show the hat symbols in a list view.
                SymbolListAdapter hatAdapter = new SymbolListAdapter(Activity, _hatSymbolInfos);

                // Handle selection change events for the hat symbol adapter.
                hatAdapter.OnSymbolSelectionChanged += async (sender, e) =>
                {
                    // Get the key for the selected hat (if any) then update the symbol.
                    if (hatAdapter.SelectedSymbolInfo != null)
                    {
                        _selectedHatKey = hatAdapter.SelectedSymbolInfo?.Key;
                        await UpdateSymbol();
                    }
                };

                // Assign the adapter to the list view.
                _hatList.Adapter = hatAdapter;

                // Create a label to for the "hat" symbol list.
                TextView hatLabel = new TextView(ctx);
                hatLabel.SetText("Hat", TextView.BufferType.Normal);
                hatLabel.Gravity = GravityFlags.Center;
                hatLabel.TextSize = 15.0f;
                hatLabel.SetTextColor(Android.Graphics.Color.Black);

                // Add the hat label and list to the dialog.
                dialogView.AddView(hatLabel);
                dialogView.AddView(_hatList);

                // Add a preview image for the symbol.
                _symbolPreviewImage = new ImageView(ctx);
                dialogView.AddView(_symbolPreviewImage);

                // A button to set a yellow symbol color.
                Button yellowButton = new Button(ctx);
                yellowButton.SetBackgroundColor(Color.Yellow);

                // Handle the button click event to set the current color and update the symbol.
                yellowButton.Click += async (sender, e) =>
                {
                    _faceColor = Color.Yellow; await UpdateSymbol();
                };

                // A button to set a green symbol color.
                Button greenButton = new Button(ctx);
                greenButton.SetBackgroundColor(Color.Green);

                // Handle the button click event to set the current color and update the symbol.
                greenButton.Click += async (sender, e) =>
                {
                    _faceColor = Color.Green; await UpdateSymbol();
                };

                // A button to set a pink symbol color.
                Button pinkButton = new Button(ctx);
                pinkButton.SetBackgroundColor(Color.Pink);

                // Handle the button click event to set the current color and update the symbol.
                pinkButton.Click += async (sender, e) =>
                {
                    _faceColor = Color.Pink; await UpdateSymbol();
                };

                // Add the color selection buttons to a horizontal layout.
                LinearLayout colorLayout = new LinearLayout(ctx)
                {
                    Orientation = Orientation.Horizontal
                };
                colorLayout.SetPadding(10, 20, 10, 20);
                colorLayout.AddView(yellowButton);
                colorLayout.AddView(greenButton);
                colorLayout.AddView(pinkButton);

                // Add the color selection buttons to the dialog.
                dialogView.AddView(colorLayout);

                // Create a slider (SeekBar) to adjust the symbol size.
                SeekBar sizeSlider = new SeekBar(ctx)
                {
                    // Set a maximum slider value of 60 and a current value of 20.
                    Max = 60,
                    Progress = 20
                };

                // Create a label to show the selected symbol size (preview size won't change).
                TextView sizeLabel = new TextView(ctx)
                {
                    Gravity = GravityFlags.Right,
                    TextSize = 10.0f,
                    Text = $"Size:{_symbolSize:0}"
                };
                sizeLabel.SetTextColor(Color.Black);

                // When the slider value (Progress) changes, update the symbol with the new size.
                sizeSlider.ProgressChanged += (s, e) =>
                {
                    // Read the slider value and limit the minimum size to 8.
                    _symbolSize = (e.Progress < 8) ? 8 : e.Progress;

                    // Show the selected size in the label.
                    sizeLabel.Text = $"Size:{_symbolSize:0}";
                };

                // Create a horizontal layout to show the slider and label.
                LinearLayout sliderLayout = new LinearLayout(ctx)
                {
                    Orientation = Orientation.Horizontal
                };
                sliderLayout.SetPadding(10, 10, 10, 10);
                sizeSlider.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, 1.0f);

                // Add the size slider and label to the layout.
                sliderLayout.AddView(sizeSlider);
                sliderLayout.AddView(sizeLabel);

                // Add the size controls to the dialog layout.
                dialogView.AddView(sliderLayout);

                // Add a button to close the dialog and use the current symbol.
                Button selectSymbolButton = new Button(ctx)
                {
                    Text = "OK"
                };

                // Handle the button click to dismiss the dialog and pass back the symbol.
                selectSymbolButton.Click += SelectSymbolButtonClick;

                // Add the button to the dialog.
                dialogView.AddView(selectSymbolButton);
            }
            catch (Exception ex)
            {
                // Show the exception message.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }

            // Show a preview of the current symbol (created previously or the default face).
            UpdateSymbol();

            // Return the new view for display.
            return dialogView;
        }

        private async void ReadMobileStyle(string mobileStyleFilePath)
        {
            try
            {
                // Make sure the file exists.
                if (!System.IO.File.Exists(mobileStyleFilePath))
                {
                    throw new System.IO.FileNotFoundException("Mobile style file not found at " + mobileStyleFilePath);
                }

                // Open the mobile style file at the path provided.
                _emojiStyle = await SymbolStyle.OpenAsync(mobileStyleFilePath);

                // Get the default style search parameters.
                SymbolStyleSearchParameters searchParams = await _emojiStyle.GetDefaultSearchParametersAsync();

                // Search the style with the default parameters to return a list of all symbol results.
                IList<SymbolStyleSearchResult> styleResults = await _emojiStyle.SearchSymbolsAsync(searchParams);

                // Loop through the results and put symbols into the appropriate list according to category (eyes, mouth, hat).
                foreach (SymbolStyleSearchResult result in styleResults)
                {
                    // Get the result symbol as a multilayer point symbol.
                    MultilayerPointSymbol multiLayerSym = result.Symbol as MultilayerPointSymbol;

                    // Create a swatch for the symbol and use it to create a bitmap image.
                    RuntimeImage swatch = await multiLayerSym.CreateSwatchAsync();
                    Bitmap symbolImage = await swatch.ToImageSourceAsync();

                    // Check the symbol category.
                    switch (result.Category)
                    {
                        // Add a new SymbolLayerInfo to represent the symbol and add it to its category list.
                        // SymbolLayerInfo is a custom class with properties for the symbol name, swatch image, and unique key.
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
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                // Show the exception message.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error reading style");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        // A function to get a multilayer point symbol that contains all selected symbol layers.
        private async Task<MultilayerPointSymbol> GetCurrentSymbol()
        {
            MultilayerPointSymbol faceSymbol = null;

            try
            {
                // Create a new list of strings with the key for the base (face) symbol and any additional selected layers.
                List<string> symbolKeys = new List<string>
                {
                    _baseSymbolKey, _selectedEyesKey, _selectedMouthKey, _selectedHatKey
                };

                // Use the keys to get the symbol layers as a multilayer symbol.
                faceSymbol = await _emojiStyle.GetSymbolAsync(symbolKeys) as MultilayerPointSymbol;

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
            }
            catch (Exception ex)
            {
                // Show the exception message.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error creating symbol");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }

            // Return the constructed symbol.
            return faceSymbol;
        }

        // Get the current symbol and update the preview image.
        private async Task UpdateSymbol()
        {
            // Call a function to get the currrent multilayer point symbol from the selected layers.
            MultilayerPointSymbol emojiSymbol = await GetCurrentSymbol();

            try 
            { 
                // Use a swatch from the symbol to create an image.
                RuntimeImage swatch = await emojiSymbol.CreateSwatchAsync(100, 100, 96, System.Drawing.Color.White);
                Bitmap symbolImage = await swatch.ToImageSourceAsync();

                // Display the preview image.
                _symbolPreviewImage.SetImageBitmap(symbolImage);
            }
            catch (Exception ex)
            {
                // Show the exception message.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error creating preview");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        // A click handler for the button that dismisses the dialog.
        private async void SelectSymbolButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get the current symbol.
                MultilayerPointSymbol selectedSymbol = await GetCurrentSymbol();

                // Create a new OnSymbolCompleteEventArgs object to store the selected symbol.
                OnSymbolCompleteEventArgs symbolSelectedArgs = new OnSymbolCompleteEventArgs(selectedSymbol);

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

    // A custom EventArgs object to store the multilayer point symbol.
    public class OnSymbolCompleteEventArgs
    {
        // A property that stores the multilayer point symbol composed of the selected style symbol layers.
        public MultilayerPointSymbol SelectedPointSymbol { get; private set; }

        // Pass the multilayer point symbol to the constructor.
        public OnSymbolCompleteEventArgs(MultilayerPointSymbol symbol)
        {
            SelectedPointSymbol = symbol;
        }
    }

    // A class that defines a custom list adapter to show symbol layers.
    public class SymbolListAdapter : BaseAdapter<SymbolLayerInfo>
    {
        // Store the current activity.
        private readonly Activity _ctx;

        // List of symbol layer infos.
        private readonly List<SymbolLayerInfo> _symbolLayerInfos;

        // The selected symbol layer info.
        public SymbolLayerInfo SelectedSymbolInfo { get; set; }

        // An event that notifies when the selected symbol has changed.
        public event EventHandler<EventArgs> OnSymbolSelectionChanged;

        // Constructor that takes the current activity and a list of symbol infos.
        public SymbolListAdapter(Activity context, List<SymbolLayerInfo> symbolInfos) : base()
        {
            _ctx = context;
            _symbolLayerInfos = symbolInfos;
        }

        // Get the symbol info at the specified position in the list.
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
            // Create an ActivityListItem, since it contains an image to display the symbol swatch.
            View listItem = _ctx.LayoutInflater.Inflate(Android.Resource.Layout.ActivityListItem, null);

            // Display a symbol swatch if one is available, otherwise display the text "None".
            if (_symbolLayerInfos.ElementAt(position).Image != null)
            {
                // Set the list item icon with the symbol swatch image.
                listItem.FindViewById<ImageView>(Android.Resource.Id.Icon).SetImageBitmap(_symbolLayerInfos.ElementAt(position).Image);
            }
            else
            {
                // Display the text "None". Choosing this item will clear the symbol layer for that category.
                listItem.FindViewById<TextView>(Android.Resource.Id.Text1).SetText("None", TextView.BufferType.Normal);
            }

            // Handle the list item click event.
            listItem.Click += (sender, e) =>
            {
                // Get the clicked symbol layer info.
                SelectedSymbolInfo = _symbolLayerInfos.ElementAt(position);

                // Fire an event to notify clients that a symbol layer selection has changed.
                OnSymbolSelectionChanged?.Invoke(this, new EventArgs());
            };

            return listItem;
        }
    }

    // A class to contain information about a symbol from a style.
    public class SymbolLayerInfo
    {
        // A bitmap preview of the symbol.
        public Bitmap Image;

        // The name of the symbol in the style.
        public string Name;

        // A key that uniquely identifies the symbol in the style.
        public string Key;

        // Take the symbol info property values in the constructor.
        public SymbolLayerInfo(string name, Bitmap image, string key)
        {
            Name = name;
            Image = image;
            Key = key;
        }
    }
}