using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to use graphics extrusion to stretch a flat 2D shape vertically to create a 3D object.
	/// </summary>
	/// <title>3D Graphics Extrusion</title>
	/// <category>Scene</category>
	/// <subcategory>Graphics</subcategory>
	public partial class GraphicsExtrusion : UserControl
	{
		public GraphicsExtrusion()
		{
			InitializeComponent();
			Initialize();
		}

		private async void Initialize()
		{
			try
			{
				CreateExtrusionInfos();

				// Set initial viewpoint
				var viewpoint = new ViewpointCenter(new MapPoint(-96, 39), 15000000);
				await MySceneView.SetViewAsync(viewpoint);

				// Query states with statistical attributes
				var queryTask = new QueryTask(
					new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));
				Query query = new Query("1=1");
				query.OutFields.Add("STATE_NAME");
				query.OutFields.Add("AGE_5_17");
				query.OutFields.Add("AGE_18_21");
				query.OutFields.Add("AGE_22_29");
				query.OutFields.Add("AGE_30_39");
				query.OutFields.Add("AGE_40_49");
				query.OutFields.Add("AGE_50_64");
				query.OutFields.Add("AGE_65_UP");

				var result = await queryTask.ExecuteAsync(query);

				var states = new GraphicCollection();
				foreach (var state in result.FeatureSet.Features)
					states.Add(new Graphic(state.Geometry, state.Attributes));

				// Make sure that all layers are loaded
				await MySceneView.LayersLoadedAsync();

				// Set graphics to the overlay
				var statesOverlay = MySceneView.GraphicsOverlays["statesOverlay"];
				statesOverlay.GraphicsSource = states;

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Graphics Extrusion Sample");
			}
		}

		// Create list of extrusion infos that contains rendering info
		private void CreateExtrusionInfos()
		{
			var renderers = new List<RenderingInfo>();
			renderers.Add(new RenderingInfo(
				"Age 5 to 17", "[AGE_5_17]", Colors.DarkBlue));

			renderers.Add(new RenderingInfo(
				"Age 18 to 21", "[AGE_18_21]", Colors.DarkCyan));

			renderers.Add(new RenderingInfo(
				"Age 22 to 29", "[AGE_22_29]", Colors.DarkGoldenrod));

			renderers.Add(new RenderingInfo(
				"Age 30 to 39", "[AGE_30_39]", Colors.DarkGray));

			renderers.Add(new RenderingInfo(
				"Age 40 to 49", "[AGE_40_49]", Colors.DarkGreen));

			renderers.Add(new RenderingInfo(
				"Age 50 to 64", "[AGE_50_64]", Colors.DarkMagenta));

			renderers.Add(new RenderingInfo(
				"Age 65 and up", "[AGE_65_UP]", Colors.DarkOliveGreen));

			statisticsComboBox.ItemsSource = renderers;
			statisticsComboBox.SelectedIndex = 0;
		}

		private void statisticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var statesOverlay = MySceneView.GraphicsOverlays["statesOverlay"];
			var renderingInfo = (e.AddedItems[0] as RenderingInfo);
			var renderer = statesOverlay.Renderer as SimpleRenderer;

			// Change rendering information based on the selection
			(renderer.Symbol as SimpleFillSymbol).Color = renderingInfo.Color;
			renderer.SceneProperties.ExtrusionExpression = renderingInfo.ExtrusionExpression;
		}

		// Simple container class for rendering information
		public class RenderingInfo
		{
			public RenderingInfo(string displayName, string extrusionExpression, Color color)
			{
				DisplayName = displayName;
				ExtrusionExpression = extrusionExpression;
				Color = color;
			}

			public string DisplayName { get; private set; }
			public Color Color { get; private set; }
			public string ExtrusionExpression { get; private set; }
		}
	}
}
