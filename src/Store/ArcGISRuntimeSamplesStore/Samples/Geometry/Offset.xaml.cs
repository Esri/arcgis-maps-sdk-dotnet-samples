using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates how to create an offset geometry using the Offset method of the GeometryEngine class.
	/// </summary>
	/// <title>Offset</title>
	/// <category>Geometry</category>
	public sealed partial class Offset : Page
	{
		private GraphicsOverlay _parcelOverlay;
		private GraphicsOverlay _offsetOverlay;
		private Graphic _selectedParcelGraphic;

		public Offset()
		{
			InitializeComponent();

			_parcelOverlay = MyMapView.GraphicsOverlays["ParcelsGraphicsOverlay"];
			_offsetOverlay = MyMapView.GraphicsOverlays["OffsetGraphicsOverlay"];

			InitializeOffsetTypes();
			OffsetDistanceSlider.ValueChanged += Slider_ValueChanged;
			OffsetTypeComboBox.SelectionChanged += ComboBox_SelectionChanged;
			OffsetFlattenErrorSlider.ValueChanged += Slider_ValueChanged;
			OffsetBevelRatioSlider.ValueChanged += Slider_ValueChanged;

			ControlsContainer.Visibility = Visibility.Collapsed;
		}

		void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			DoOffset();
		}

		void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DoOffset();
		}

		private void InitializeOffsetTypes()
		{
			OffsetTypeComboBox.ItemsSource = new List<OffsetType> { OffsetType.Bevel, OffsetType.Miter, OffsetType.Round, OffsetType.Square };
			OffsetTypeComboBox.SelectedIndex = 0;
		}

		private async Task SelectParcelForOffsetAsync()
		{
			ResetButton.IsEnabled = false;

			try
			{
				_offsetOverlay.Graphics.Clear();

				var pointGeom = await MyMapView.Editor.RequestPointAsync();
				var screenPnt = MyMapView.LocationToScreen(pointGeom);

				_selectedParcelGraphic = await
					_parcelOverlay.HitTestAsync(MyMapView, screenPnt);

				DoOffset();
			}
			catch (TaskCanceledException) { }
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message).ShowAsync();
			}

			ResetButton.IsEnabled = true;
		}

		private void DoOffset()
		{
			if (_selectedParcelGraphic != null)
			{
				_offsetOverlay.Graphics.Clear();
				try
				{
					var offsetGeom = GeometryEngine.Offset(
							_selectedParcelGraphic.Geometry,
							OffsetDistanceSlider.Value,
							(OffsetType)OffsetTypeComboBox.SelectedItem,
							OffsetBevelRatioSlider.Value,
							OffsetFlattenErrorSlider.Value);

					if (offsetGeom != null)
						_offsetOverlay.Graphics.Add(new Graphic { Geometry = offsetGeom });
				}
				catch (Exception ex)
				{
					var _x = new MessageDialog(ex.Message).ShowAsync();
				}
			}
		}

		private async void ResetButton_Click(object sender, RoutedEventArgs e)
		{
			await SelectParcelForOffsetAsync();
		}

		private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (_parcelOverlay != null && _parcelOverlay.Graphics.Count == 0)
			{
				QueryTask queryTask = new QueryTask(
					new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));

				//Create a geometry to use as the extent within which parcels will be returned
				var contractRatio = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent.Width / 6;

				var extentGeometry = new Envelope(
					-83.3188395774275,
					42.61428312652851,
					-83.31295664068958,
					42.61670913269855,
					SpatialReferences.Wgs84);

				Query query = new Query(extentGeometry);
				query.ReturnGeometry = true;
				query.OutSpatialReference = SpatialReferences.WebMercator;

				try
				{
					var results = await queryTask.ExecuteAsync(query, CancellationToken.None);
					foreach (Graphic g in results.FeatureSet.Features)
					{
						_parcelOverlay.Graphics.Add(g);
					}
					LoadingParcelsIndicator.IsActive = false;
					LoadingParcelsContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
					ControlsContainer.Visibility = Visibility.Visible;
				}
				catch (Exception ex)
				{
					var _x = new MessageDialog(ex.Message).ShowAsync();
				}
			}
			await SelectParcelForOffsetAsync();
		}
	}
}

