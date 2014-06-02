using Esri.ArcGISRuntime.AdvancedSymbology;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples.Symbology
{
	/// <summary>
	/// Sample shows how to use search the Mil2525C symbol dictionary.
	/// </summary>
	/// <title>Symbol Dictionary Search</title>
	/// <category>Symbology</category>
	/// <subcategory>Advanced</subcategory>
	public partial class SymbolDictionarySearchSample : UserControl, INotifyPropertyChanged
	{
		private SymbolViewModel _selectedSymbol;
		private SymbolDictionary _symbolDictionary;
		private IList<string> _keywords;
		private int _imageSize;
		private MessageLayer _messageLayer;

		public SymbolDictionarySearchSample()
		{
			// Create a new SymbolDictionary instance 
			_symbolDictionary = new SymbolDictionary(SymbolDictionaryType.Mil2525c);

			// Collection of strings to hold the selected symbol dictionary keywords
			SelectedKeywords = new ObservableCollection<string>();
			_keywords = _symbolDictionary.Keywords.ToList();

			// Collection of view models for the displayed list of symbols
			Symbols = new ObservableCollection<SymbolViewModel>();

			// Set the DataContext for binding
			DataContext = this;
			InitializeComponent();

			// Set the image size
			_imageSize = 40;

			// Get reference to MessageLayer to use with messages
			_messageLayer = mapView.Map.Layers.OfType<MessageLayer>().First();
		}

		// Search results 
		public ObservableCollection<SymbolViewModel> Symbols { get; private set; }
		
		// Currently selected keywords
		public ObservableCollection<string> SelectedKeywords { get; private set; }

		// All keywords in alphabetical order
		public IEnumerable<string> Keywords { get { return new[] { "" }.Concat(_keywords.OrderBy(k => k)); } }
		
		// All style files used in symbol dictionary
		public IEnumerable<string> StyleFiles { get { return new[] { "" }.Concat(_symbolDictionary.Filters["StyleFile"]); } }

		// All categories used in symbol dictionary
		public IEnumerable<string> Categories { get { return new[] { "" }.Concat(_symbolDictionary.Filters["Category"]); } }
		
		// All geometry types used in symbol dictionary
		public IEnumerable<string> GeometryTypes { get { return new[] { "" }.Concat(_symbolDictionary.Filters["GeometryType"]); } }

		// SelectedChanged event handler for the ListBoxes
		private void SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Search();
		}

		// Remove a keyword from the selected keywords collection and rerun the search
		private void RemoveKeyword(object sender, RoutedEventArgs e)
		{
			Button b = (Button)sender;
			string kw = (string)b.DataContext;
			SelectedKeywords.Remove(kw);
			Search();
		}

		// SelectionChanged event handler for the keywords ListBox
		private void KeywordSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (string.IsNullOrEmpty((string)cmbKeyword.SelectedValue))
				return;

			var kw = cmbKeyword.SelectedValue.ToString();

			// Add keyword to selected list if it's not already there
			if (SelectedKeywords.FirstOrDefault(s => s == kw) == null)
				SelectedKeywords.Add(kw);
			Search();
		}

		// Request geometry and new message to the layer
		private async Task AddSymbolAsync(DrawShape requestedShape)
		{
			try
			{
				// Keep adding messages until next symbol is selected
				while (true)
				{
					var geometry = await mapView.Editor.RequestShapeAsync(requestedShape, null, null);

					// Create a new message
					Message msg = new Message();

					// Set the ID and other parts of the message
					msg.Id = Guid.NewGuid().ToString();
					msg.Add("_type", "position_report");
					msg.Add("_action", "update");
					msg.Add("_wkid", "3857");
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
							foreach (var pt in polygon.Rings[0])
								cpts += ";" + pt.X.ToString(CultureInfo.InvariantCulture) + "," + pt.Y.ToString(CultureInfo.InvariantCulture);
							msg.Add("_control_points", cpts);
							break;
						case DrawShape.Polyline:
							Polyline polyline = geometry as Polyline;
							cpts = string.Empty;
							foreach (var pt in polyline.Paths[0])
								cpts += ";" + pt.X.ToString(CultureInfo.InvariantCulture) + "," + pt.Y.ToString(CultureInfo.InvariantCulture);
							msg.Add("_control_points", cpts);
							break;
					}

					// Process the message
					if (!_messageLayer.ProcessMessage(msg))
						MessageBox.Show("Failed to process message.");
				}
			}
			catch (TaskCanceledException taskCanceledException)
			{
				// Requsting geometry was canceled.
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Symbol Dictionary Search Sample");
			}
		}

		// Sets the currently selected symbol
		private void SymbolListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count != 1)
				return;

			_selectedSymbol = e.AddedItems[0] as SymbolViewModel;

			Dictionary<string, string> values = (Dictionary<string, string>)_selectedSymbol.Model.Values;
			string geometryControlType = values["GeometryConversionType"];
			DrawShape requestedShape = DrawShape.Point;

			// Note that not all Geometry types are handled here
			switch (geometryControlType)
			{
				case "Point":
					requestedShape = DrawShape.Point;
					break;
				case "Polyline":
				case "PolylineWithTail":
				case "TripleArrow":
				case "ArrowWithOffset":
				case "ParallelLinesMidline":
				case "UOrTShape":
				case "T":
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
					MessageBox.Show("Selected symbol is not supported in this sample");
					return;
			}

			// Enable adding symbols to the map
			var _ = AddSymbolAsync(requestedShape);
		}

		// Function to search the symbol dictionary based on the selected value in the style file, category and/or geometry type ListBoxes
		private void Search()
		{
			Dictionary<string, string> filters = new Dictionary<string, string>();

			// Set filters if there is some selected
			if (!string.IsNullOrEmpty((string)cmbStyleFile.SelectedValue))
				filters["StyleFile"] = cmbStyleFile.SelectedValue.ToString();
				
			if (!string.IsNullOrEmpty((string)cmbCategory.SelectedValue))
				filters["Category"] = cmbCategory.SelectedValue.ToString();

			if (!string.IsNullOrEmpty((string)cmbGeometryType.SelectedValue))
				filters["GeometryType"] = cmbGeometryType.SelectedValue.ToString();

			// Clear the current Symbols collection
			Symbols.Clear();

			// Perform the search applying any selected keywords and filters 
			IEnumerable<SymbolProperties> symbols = _symbolDictionary.FindSymbols(SelectedKeywords, filters);
			var allSymbols = symbols.ToList();

			// Update the list of applicable keywords (excluding any keywords that are not on the current result set)
			if (SelectedKeywords == null || SelectedKeywords.Count == 0)
			{
				_keywords = _symbolDictionary.Keywords.Where(k => !IsSymbolId(k)).ToList();
			}
			else
			{
				IEnumerable<string> allSymbolKeywords = allSymbols.SelectMany(s => s.Keywords);
				_keywords = allSymbolKeywords.Distinct()
					.Except(SelectedKeywords)
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

        public string StyleFile
        {
            get { return _model.Values["StyleFile"].ToString(); }
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
