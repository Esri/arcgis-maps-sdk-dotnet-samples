using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geometry</category>
	public sealed partial class LabelPoints : Page
    {
        GraphicsLayer myGraphicsLayer;

        PictureMarkerSymbol pictureMarkerSymbol;
        Geometry inputGeom;
        public LabelPoints()
        {
            InitializeComponent();

			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-118.331, 33.7, -116.75, 34, SpatialReferences.Wgs84));
            myGraphicsLayer = mapView1.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
            InitializePictureMarkerSymbol();

            mapView1.Loaded += mapView1_Loaded;

        }
        private async void InitializePictureMarkerSymbol()
        {
            try
            {
                var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/x-24x24.png"));
                var imageSource = await imageFile.OpenReadAsync();
                pictureMarkerSymbol = LayoutRoot.Resources["MyPictureMarkerSymbol"] as PictureMarkerSymbol;
                await pictureMarkerSymbol.SetSourceAsync(imageSource);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        async void mapView1_Loaded(object sender, RoutedEventArgs e)
        {

            await DoLabelPoints();

        }

        private async Task DoLabelPoints()
        {
            ResetButton.IsEnabled = false;


            try
            {
                if (mapView1.Editor.IsActive)
                    mapView1.Editor.Cancel.Execute(null);

                //Get the input polygon geometry from the user
                inputGeom = await mapView1.Editor.RequestShapeAsync(DrawShape.Polygon);

                if (inputGeom != null)
                {
                    //Add the polygon drawn by the user
                    var g = new Graphic
                    {
                        Geometry = inputGeom,
                    };
                    myGraphicsLayer.Graphics.Add(g);


                    //Get the label point for the input geometry
                    var labelPointGeom = GeometryEngine.LabelPoint(inputGeom);

                    if (labelPointGeom != null)
                    {
                        myGraphicsLayer.Graphics.Add(new Graphic { Geometry = labelPointGeom, Symbol = pictureMarkerSymbol });

                    }
                }
                await DoLabelPoints();
            }
            catch (Exception)
            {

            }
            ResetButton.IsEnabled = true;

        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {

            myGraphicsLayer.Graphics.Clear();
            await DoLabelPoints();

        }


    }
}
