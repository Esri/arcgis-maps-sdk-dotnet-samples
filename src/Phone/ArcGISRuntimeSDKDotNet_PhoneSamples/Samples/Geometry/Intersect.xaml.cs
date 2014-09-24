﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geometry</category>
	public sealed partial class Intersect : Page
    {
        GraphicsLayer parcelGraphicsLayer;
        GraphicsLayer intersectGraphicsLayer;

        Random random;
        Geometry inputGeom;
        public Intersect()
        {
            InitializeComponent();

			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-83.3188396, 42.61428312, -83.31295664, 42.6167091, SpatialReferences.Wgs84));
            parcelGraphicsLayer = mapView1.Map.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
            intersectGraphicsLayer = mapView1.Map.Layers["IntersectGraphicsLayer"] as GraphicsLayer;
            random = new Random();
        }

        private async Task DoIntersection()
        {
            intersectGraphicsLayer.Graphics.Clear();
            ResetButton.IsEnabled = false;


            try
            {
                if (mapView1.Editor.IsActive)
                    mapView1.Editor.Cancel.Execute(null);

				// Wait for user to draw a polygon
				Polygon userpoly = await mapView1.Editor.RequestShapeAsync(DrawShape.Polygon) as Polygon;

				Polygon inputGeom = GeometryEngine.NormalizeCentralMeridian(userpoly) as Polygon;

                if (inputGeom != null)
                {
                    //Add the polygon drawn by the user
                    var g = new Graphic
                    {
                        Geometry = inputGeom,
                        Symbol = new SimpleFillSymbol { Outline = new SimpleLineSymbol { Width = 2, Color = Colors.Gray }, Style = SimpleFillStyle.Null }
                    };
                    intersectGraphicsLayer.Graphics.Add(g);


                    //Optional - Simplify the input geometry
                    inputGeom = GeometryEngine.Simplify(inputGeom) as Polygon;

                    //Do the intersection for each of the graphics in the parcels layer
                    foreach (var parcel in parcelGraphicsLayer.Graphics)
                    {
                        var intersected = GeometryEngine.Intersection(inputGeom, parcel.Geometry);

                        if (intersected != null)
                        {
                            var color = Color.FromArgb((byte)100, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
                            intersectGraphicsLayer.Graphics.Add(new Graphic
                            {
                                Geometry = intersected,
                                Symbol = new SimpleFillSymbol
                                {
                                    Color = color,
                                    Outline = new SimpleLineSymbol { Color = Colors.Black, Width = 2 }
                                }
                            });

                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            ResetButton.IsEnabled = true;

        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {

            await DoIntersection();

        }

        private async void mapView1_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.Layer.ID == "ParcelsGraphicsLayer")
            {
                if (parcelGraphicsLayer != null && parcelGraphicsLayer.Graphics.Count == 0)
                {
                    QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));

                    //Create a geometry to use as the extent within which parcels will be returned
                    var contractRatio = mapView1.Extent.Width / 6;
                    var extentGeometry = new Envelope(mapView1.Extent.GetCenter().X - contractRatio,
                        mapView1.Extent.GetCenter().Y - contractRatio,
                        mapView1.Extent.GetCenter().X + contractRatio,
                        mapView1.Extent.GetCenter().Y + contractRatio,
						mapView1.SpatialReference);
                    Query query = new Query(extentGeometry);
                    query.ReturnGeometry = true;
                    query.OutSpatialReference = mapView1.SpatialReference;


                    var results = await queryTask.ExecuteAsync(query, CancellationToken.None);
                    foreach (Graphic g in results.FeatureSet.Features)
                    {
                        parcelGraphicsLayer.Graphics.Add(g);
                    }
                }
                await DoIntersection();
            }
        }


    }
}
