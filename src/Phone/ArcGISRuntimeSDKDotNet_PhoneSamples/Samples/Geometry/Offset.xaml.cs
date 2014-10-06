using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geometry</category>
	public sealed partial class Offset : Page
    {
        GraphicsLayer parcelGraphicsLayer;
        GraphicsLayer offsetGraphicsLayer;
        Graphic selectedParcelGraphic;
        public Offset()
        {
            InitializeComponent();

			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-9275076, 5253226, -9274274, 5253886, SpatialReferences.WebMercator));
            parcelGraphicsLayer = mapView1.Map.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
            offsetGraphicsLayer = mapView1.Map.Layers["OffsetGraphicsLayer"] as GraphicsLayer;

            InitializeOffsetTypes();
            OffsetDistanceSlider.ValueChanged += Slider_ValueChanged;
            OffsetTypeComboBox.SelectionChanged += ComboBox_SelectionChanged;
            OffsetFlattenErrorSlider.ValueChanged += Slider_ValueChanged;
            OffsetBevelRatioSlider.ValueChanged += Slider_ValueChanged;

            ControlsContainer.Visibility = Visibility.Collapsed;
        }

        void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
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

        private async Task SelectParcelForOffset()
        {
            ResetButton.IsEnabled = false;


            try
            {
                offsetGraphicsLayer.Graphics.Clear();

                var pointGeom = await mapView1.Editor.RequestPointAsync();
                var screenPnt = mapView1.LocationToScreen(pointGeom);

                selectedParcelGraphic = await
                    parcelGraphicsLayer.HitTestAsync(mapView1, screenPnt);

                DoOffset();
            }
            catch (Exception)
            {

            }
            ResetButton.IsEnabled = true;

        }

        private void DoOffset()
        {
            if (selectedParcelGraphic != null)
            {
                offsetGraphicsLayer.Graphics.Clear();
                try
                {
                    var offsetGeom = GeometryEngine.Offset(
                        selectedParcelGraphic.Geometry,
                        OffsetDistanceSlider.Value,
                        (OffsetType)OffsetTypeComboBox.SelectedItem,
                         OffsetBevelRatioSlider.Value,
                         OffsetFlattenErrorSlider.Value
                        );

                    if (offsetGeom != null)
                    {
                        offsetGraphicsLayer.Graphics.Add(new Graphic { Geometry = offsetGeom });
                    }
                }
                catch (Exception ex)
                {

                    var dlg = new Windows.UI.Popups.MessageDialog(ex.Message);
                    var _ = dlg.ShowAsync();
                }
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {

            await SelectParcelForOffset();

        }

        private async void mapView1_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
        {
            if (e.Layer.ID == "ParcelsGraphicsLayer")
            {
                if (parcelGraphicsLayer != null && parcelGraphicsLayer.Graphics.Count == 0)
                {
                    QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));

                    //Create a geometry to use as the extent within which parcels will be returned
                    var contractRatio = mapView1.Extent.Width / 6;
                    var extentGeometry = new Envelope(-83.3188395774275, 42.61428312652851, -83.31295664068958, 42.61670913269855, SpatialReferences.Wgs84);
                    Query query = new Query(extentGeometry);
                    query.ReturnGeometry = true;
                    query.OutSpatialReference = mapView1.SpatialReference;

                    try
                    {

                        var results = await queryTask.ExecuteAsync(query, CancellationToken.None);
                        foreach (Graphic g in results.FeatureSet.Features)
                        {
                            parcelGraphicsLayer.Graphics.Add(g);
                        }
                        LoadingParcelsIndicator.IsActive = false;
                        LoadingParcelsContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        ControlsContainer.Visibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {

                        var dlg = new Windows.UI.Popups.MessageDialog(ex.Message);
						var _ = dlg.ShowAsync();
                    }
                }
                await SelectParcelForOffset();
            }
        }


    }
}
