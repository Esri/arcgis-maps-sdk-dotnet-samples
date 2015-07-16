using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample shows how to add a FeatureLayer from a local .geodatabase file to the scene.
	/// </summary>
	/// <title>3D Feature Layer from Local Geodatabase</title>
	/// <category>Scene</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerFromLocalGeodatabase3d : UserControl
	{
		private const string GeodatabasePath = @"..\..\..\samples-data\maps\usa.geodatabase";

		public FeatureLayerFromLocalGeodatabase3d()
		{
			InitializeComponent();
			CreateFeatureLayers();
		}

		private async void CreateFeatureLayers()
		{
			try
			{
				var geodatabase = await Geodatabase.OpenAsync(GeodatabasePath);

				Envelope extent = null;
				foreach (var table in geodatabase.FeatureTables)
				{
					var featureLayer = new FeatureLayer()
					{
						ID = table.Name,
						DisplayName = table.Name,
						FeatureTable = table
					};

					if (!Geometry.IsNullOrEmpty(table.ServiceInfo.Extent))
					{
						if (Geometry.IsNullOrEmpty(extent))
							extent = table.ServiceInfo.Extent;
						else
							extent = extent.Union(table.ServiceInfo.Extent);
					}

					MySceneView.Scene.Layers.Add(featureLayer);
				}

				await MySceneView.SetViewAsync(new Camera(new MapPoint(-99.343, 26.143, 5881928.401), 2.377, 10.982));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error creating feature layer: " + ex.Message, "Samples");
			}
		}
	}
}
