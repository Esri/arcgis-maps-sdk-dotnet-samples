using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology.Specialized;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ArcGISRuntime.Samples.Phone.Samples.Symbology.Specialized
{
	/// <summary>
	/// Sample shows how to use search the Mil2525C symbol dictionary.
	/// </summary>
	/// <title>Symbol Dictionary Search</title>
	/// <category>Symbology</category>
	/// <subcategory>Advanced</subcategory>	
	/// <requiresSymbols>true</requiresSymbols>
	public sealed partial class SymbolDictionarySearchSample : Page, INotifyPropertyChanged
	{
		private SymbolDictionary _symbolDictionary;
		private IList<string> _keywords;
		private int _imageSize;
		private MessageLayer _messageLayer;

		public SymbolDictionarySearchSample()
		{
			InitializeComponent();
			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			Init();
		}

        private async void Init()
        {
            // Wait until all layers are loaded
            await MyMapView.LayersLoadedAsync();

			bool isSymbolDictionaryInitialized = false;
			try
			{
				// Create a new SymbolDictionary instance 
				_symbolDictionary = new SymbolDictionary(SymbolDictionaryType.Mil2525c);
				isSymbolDictionaryInitialized = true;
			}
			catch { }

			if (!isSymbolDictionaryInitialized)
			{
				await new MessageDialog("Failed to create symbol dictionary.", "Symbol Dictionary Search Sample").ShowAsync();
				return;
			}

            // Remove any empty strings space from keywords
            _keywords = _symbolDictionary.Keywords.OrderBy(k => k).ToList();
            _keywords = _keywords.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            // Collection of view models for the displayed list of symbols
            Symbols = new ObservableCollection<SymbolViewModel>();

            // Set the DataContext for binding
            DataContext = this;

            // Set the image size
            _imageSize = 64;

            // Get reference to MessageLayer to use with messages
            _messageLayer = MyMapView.Map.Layers.OfType<MessageLayer>().First();
        }

		// Search results 
		public ObservableCollection<SymbolViewModel> Symbols { get; private set; }

		// Currently selected keyword
		private string _selectedKeyword;
		public string SelectedKeyword
		{
			get { return _selectedKeyword; }
			set {
				if (_selectedKeyword == value)
					return;
				_selectedKeyword = value;
				Search();
				RaisePropertyChanged("SelectedKeyword"); }
		}

		// Currently selected symbol
		private SymbolViewModel _selectedSymbol;
		public SymbolViewModel SelectedSymbol
		{
			get { return _selectedSymbol; }
			set
			{
				if (_selectedSymbol == value)
					return;
				_selectedSymbol = value;
				AddSymbolAsync(_selectedSymbol);
				RaisePropertyChanged("SelectedSymbol");
			}
		}

		// All keywords in alphabetical order
		public IEnumerable<string> Keywords
		{
			get { return _keywords; }
			set
			{
				_keywords = value.ToList();
				RaisePropertyChanged("Keywords");
			}
		}

		// Request geometry and new message to the layer
		private async void AddSymbolAsync(SymbolViewModel symbolViewModel)
		{
			try
			{
				Dictionary<string, string> values = (Dictionary<string, string>)symbolViewModel.Model.Values;
				string geometryControlType = values["GeometryType"];
				DrawShape requestedShape = DrawShape.Point;

				switch (geometryControlType)
				{
					case "Point":
						requestedShape = DrawShape.Point;
						break;
					case "Line":
						requestedShape = DrawShape.Polyline;
						break;
					case "Polygon":
						requestedShape = DrawShape.Polygon;
						break;
					case "Circle":
						requestedShape = DrawShape.Circle;
						break;
					case "Rectangular":
						requestedShape = DrawShape.Rectangle;
						break;
					default:
						await new MessageDialog("Selected symbol is not supported in this sample", "Symbol Dictionary Search Sample").ShowAsync();
						return;
				}
				while (true)
				{
					Esri.ArcGISRuntime.Geometry.Geometry geometry = null;

					try
					{
						geometry = await MyMapView.Editor.RequestShapeAsync(requestedShape, null, null);
					}
					catch { }

					if (geometry == null)
						return;

					// Create a new message
					Message msg = new Message();

					// Set the ID and other parts of the message
					msg.Id = Guid.NewGuid().ToString();
					msg.Add("_type", "position_report");
					msg.Add("_action", "update");
					msg.Add("_wkid", MyMapView.SpatialReference.Wkid.ToString());
					msg.Add("sic", _selectedSymbol.SymbolID);
					msg.Add("uniquedesignation", "1");

					// Construct the Control Points based on the geometry type of the drawn geometry.
					switch (requestedShape)
					{
						case DrawShape.Point:
							MapPoint point = geometry as MapPoint;
							msg.Add("_control_points", point.X.ToString(CultureInfo.InvariantCulture) + "," + point.Y.ToString(CultureInfo.InvariantCulture));
							break;
						case DrawShape.Polygon:
							Polygon polygon = geometry as Polygon;
							string cpts = string.Empty;
							foreach (var pt in polygon.Parts[0].GetPoints())
								cpts += ";" + pt.X.ToString(CultureInfo.InvariantCulture) + "," + pt.Y.ToString(CultureInfo.InvariantCulture);
							msg.Add("_control_points", cpts);
							break;
						case DrawShape.Polyline:
							Polyline polyline = geometry as Polyline;
							cpts = string.Empty;
							foreach (var pt in polyline.Parts[0].GetPoints())
								cpts += ";" + pt.X.ToString(CultureInfo.InvariantCulture) + "," + pt.Y.ToString(CultureInfo.InvariantCulture);
							msg.Add("_control_points", cpts);
							break;
					}

					// Process the message
					if (!_messageLayer.ProcessMessage(msg))
						await new MessageDialog("Failed to process message.", "Symbol Dictionary Search Sample").ShowAsync();

					btnClearMap.IsEnabled = true;
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Symbol Dictionary Search Sample").ShowAsync();
			}
		}

		// Function to search the symbol dictionary based on the selected value in the style file, category and/or geometry type ListBoxes
		private void Search()
		{
			// Create empty filter dictionary, not used
			Dictionary<string, string> filters = new Dictionary<string, string>();

			// Clear the current Symbols collection
			Symbols.Clear();

			// Perform the search applying any selected keywords
			IEnumerable<SymbolProperties> symbols = _symbolDictionary.FindSymbols(new List<string>() { _selectedKeyword });
			
			var allSymbols = symbols.ToList();

			// Update the list of applicable keywords (excluding any keywords that are not on the current result set)
			if (SelectedKeyword == null)
			{
				_keywords = _symbolDictionary.Keywords.ToList().Where(k => !IsSymbolId(k)).ToList();
			}
			else
			{
				IEnumerable<string> allSymbolKeywords = allSymbols.SelectMany(s => s.Keywords);
				_keywords = allSymbolKeywords.Distinct()
					.Except(new List<string>(){SelectedKeyword})
					.Where(k => !IsSymbolId(k))
					.ToList();
			}

			RaisePropertyChanged("Keywords");

			// Add symbols to UI collection
			foreach (var s in from symbol in allSymbols select new SymbolViewModel(symbol, _imageSize))
				Symbols.Add(s);
		}

		// Do not add keywords which represent a single symbol to the Keywords list.
		private bool IsSymbolId(string keyword)
		{
			if (keyword.Length == 15)
			{
				keyword = keyword.ToUpperInvariant();

				if (!"SGWIOE".Contains(keyword[0]))
					return false;

				if (!"PUAFNSHGWMDLJKO-".Contains(keyword[1]))
					return false;

				if (!"PAGSUFXTMOEVLIRNZ-".Contains(keyword[2]))
					return false;

				if (!"APCDXF-".Contains(keyword[3]))
					return false;

				if (Enumerable.Range(4, 6).Any(i => !"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-".Contains(keyword[i])))
					return false;

				if (Enumerable.Range(10, 2).Any(i => !"ABCDEFGHIJKLMNOPQRSTUVWXYZ-*".Contains(keyword[i])))
					return false;

				if (Enumerable.Range(12, 2).Any(i => !"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-*".Contains(keyword[i])))
					return false;

				if (!"AECGNSX-".Contains(keyword[14]))
					return false;

				return true;
			}
			return false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string name)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(name));
		}

		private void ClearMap_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (_messageLayer != null)
			{
				_messageLayer.ChildLayers.Clear();
			}
			btnClearMap.IsEnabled = false;
		}
	}

	// Presents single symbol
	public class SymbolViewModel : INotifyPropertyChanged
	{
		private int _imageSize;
		private SymbolProperties _model;
		private ImageSource _image;

		public SymbolViewModel(SymbolProperties model, int imageSize)
		{
			_model = model;
			_imageSize = imageSize;
		}

		public string Name { get { return _model.Name; } }

		public string Keywords { get { return string.Join(", ", _model.Keywords); } }

		public string Category
		{
			get { return _model.Values["Category"].ToString(); }
		}

		public string SymbolID
		{
			get { return _model.Values["SymbolID"].ToString(); }
		}

		public int ImageSize
		{
			get { return _imageSize; }
		}

		public ImageSource Thumbnail
		{
			get
			{
				if (_image == null)
				{
					try
					{
						_image = _model.GetImage(_imageSize, _imageSize);
					}
					catch (Exception)
					{
						return null;
					}
				}
				return _image;
			}
		}

		public SymbolProperties Model
		{
			get { return _model; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string name)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(name));
		}
	}
}
