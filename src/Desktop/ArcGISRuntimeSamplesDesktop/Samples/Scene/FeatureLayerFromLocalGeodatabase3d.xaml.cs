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
		private const string GDB_PATH = @"..\..\..\samples-data\maps\usa.geodatabase";

		/// <summary>Construct FeatureLayerFromLocalGeodatabase sample control</summary>
		public FeatureLayerFromLocalGeodatabase3d()
		{
			InitializeComponent();
			CreateFeatureLayers();
		}

		private async void CreateFeatureLayers()
		{
			try
			{
				var gdb = await Geodatabase.OpenAsync(GDB_PATH);

				Envelope extent = null;
				foreach (var table in gdb.FeatureTables)
				{
					var flayer = new FeatureLayer()
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

					MySceneView.Scene.Layers.Add(flayer);
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
