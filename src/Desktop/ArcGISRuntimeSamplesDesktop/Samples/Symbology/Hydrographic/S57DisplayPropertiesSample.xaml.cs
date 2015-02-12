using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArcGISRuntime.Samples.Desktop.Symbology.Hydrographic
{
	/// <summary>
	/// This sample demonstrates using the global static HydrographicS52DisplayProperties object to control the display properties of an HydrographicS57Layer, such as the Color Scheme or Safety Contour Depth. 
	/// </summary>
	/// <title>S57 Display Properties</title>
	/// <category>Symbology</category>
	/// <subcategory>Hydrographic</subcategory>
	/// <requiresSymbols>true</requiresSymbols>
	public partial class S57DisplayPropertiesSample : UserControl 
	{
		public S57DisplayPropertiesSample()
		{
			InitializeComponent();

			// Create default instance of display properties and set that to DataContext for binding
			DataContext = HydrographicS52DisplayProperties.Default;
			ZoomToHydrographicLayers();
		}

		// Zoom to combined extent of the group layer that contains all hydrographic layers
		private async void ZoomToHydrographicLayers()
		{
			try
			{
				// wait until all layers are loaded
				await MyMapView.LayersLoadedAsync();

				var hydroGroupLayer = MyMapView.Map.Layers.OfType<GroupLayer>().First();
				var extent = hydroGroupLayer.ChildLayers.First().FullExtent;

				// Create combined extent from child hydrographic layers
				foreach (var layer in hydroGroupLayer.ChildLayers)
					extent = extent.Union(layer.FullExtent);

				// Zoom to full extent
				await MyMapView.SetViewAsync(extent);

				// Enable controls
				displayPropertiesArea.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		// Show error if loading layers fail
		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			MessageBox.Show(
					string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
					"Error loading layer");
		}
	}

	public class EnumValuesExtension : MarkupExtension
	{
		private readonly Type _enumType;

		public EnumValuesExtension(Type enumType)
		{
			if (enumType == null)
				throw new ArgumentNullException("enumType");
			if (!enumType.IsEnum)
				throw new ArgumentException("Argument enumType must derive from type Enum.");

			_enumType = enumType;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return Enum.GetValues(_enumType);
		}
	}

	public class FlagsEnumValueConverter : IValueConverter
	{
		private int targetValue;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int mask = (int)parameter;
			this.targetValue = (int)value;
			return ((mask & this.targetValue) != 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			this.targetValue ^= (int)parameter;
			return Enum.Parse(targetType, this.targetValue.ToString());
		}
	}
}
