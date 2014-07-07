using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to create an offset geometry using the Offset method of the GeometryEngine class.
    /// </summary>
    /// <title>Offset</title>
    /// <category>Geometry</category>
    public partial class Offset : UserControl
    {
        private GraphicsLayer parcelGraphicsLayer;
        private GraphicsLayer offsetGraphicsLayer;
        private Graphic selectedParcelGraphic;

        public Offset()
        {
            InitializeComponent();

			mapView.Map.InitialViewpoint = new Envelope(-9275076.4794, 5253225.9406, -9274273.6411, 5253885.6155, SpatialReferences.WebMercator);
            parcelGraphicsLayer = mapView.Map.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
            offsetGraphicsLayer = mapView.Map.Layers["OffsetGraphicsLayer"] as GraphicsLayer;

            InitializeOffsetTypes();
            OffsetDistanceSlider.ValueChanged += Slider_ValueChanged;
            OffsetTypeComboBox.SelectionChanged += ComboBox_SelectionChanged;
            OffsetFlattenErrorSlider.ValueChanged += Slider_ValueChanged;
            OffsetBevelRatioSlider.ValueChanged += Slider_ValueChanged;

            ControlsContainer.Visibility = Visibility.Collapsed;
        }

        void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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
            try
            {
                ResetButton.IsEnabled = false;
                offsetGraphicsLayer.Graphics.Clear();

                var pointGeom = await mapView.Editor.RequestPointAsync();
                var screenPnt = mapView.LocationToScreen(pointGeom);

                selectedParcelGraphic = await
                    parcelGraphicsLayer.HitTestAsync(mapView, screenPnt);

                DoOffset();
            }
            catch (Exception)
            {
            }
            finally
            {
                ResetButton.IsEnabled = true;
            }
        }

        private void DoOffset()
        {
            if (selectedParcelGraphic != null)
            {
                offsetGraphicsLayer.Graphics.Clear();

                try
                {
                    var offsetGeom = GeometryEngine.Offset(selectedParcelGraphic.Geometry,
                        OffsetDistanceSlider.Value, (OffsetType)OffsetTypeComboBox.SelectedItem,
                        OffsetBevelRatioSlider.Value, OffsetFlattenErrorSlider.Value);
                    if (offsetGeom != null)
                    {
                        offsetGraphicsLayer.Graphics.Add(new Graphic { Geometry = offsetGeom });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Sample Error");
                }
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            await SelectParcelForOffset();
        }

        private async void mapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.Layer.ID == "ParcelsGraphicsLayer")
            {
                if (parcelGraphicsLayer != null && parcelGraphicsLayer.Graphics.Count == 0)
                {
                    try
                    {
                        ControlsContainer.Visibility = Visibility.Collapsed;
                        LoadingParcelsContainer.Visibility = Visibility.Visible;

                        QueryTask queryTask = new QueryTask(
                            new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));

                        //Create a geometry to use as the extent within which parcels will be returned
                        var contractRatio = mapView.Extent.Width / 6;
                        var extentGeometry = new Envelope(-83.3188395774275, 42.61428312652851, -83.31295664068958, 42.61670913269855, SpatialReferences.Wgs84);

                        Query query = new Query(extentGeometry);
                        query.ReturnGeometry = true;
                        query.OutSpatialReference = mapView.SpatialReference;

                        var results = await queryTask.ExecuteAsync(query, CancellationToken.None);
                        foreach (Graphic g in results.FeatureSet.Features)
                        {
                            parcelGraphicsLayer.Graphics.Add(g);
                        }

                        ControlsContainer.Visibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading parcel data: " + ex.Message, "Sample Error");
                    }
                    finally
                    {
                        LoadingParcelsContainer.Visibility = Visibility.Collapsed;
                    }
                }

                await SelectParcelForOffset();
            }
        }
    }
}
