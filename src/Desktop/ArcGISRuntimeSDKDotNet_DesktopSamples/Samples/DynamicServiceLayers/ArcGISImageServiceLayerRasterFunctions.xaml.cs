using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Interaction logic for ArcGISImageServiceLayerRasterFunctions.xaml
    /// </summary>
    /// <title>Image Service Raster Functions</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class ArcGISImageServiceLayerRasterFunctions : UserControl
    {
        public ArcGISImageServiceLayerRasterFunctions()
        {
            InitializeComponent();
            RasterParamCombo.SelectedIndex = 0;
        }

        private void OnApplyRenderingRuleClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ArcGISImageServiceLayer lyr = map1.Layers[0] as ArcGISImageServiceLayer;
                RenderingRule renderingRule = new RenderingRule();
                renderingRule.RasterFunctionName = "Aspect";
                lyr.RenderingRule = renderingRule;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying rendering rule: " + ex.Message);
            }
        }

        private void OnMakeColorMapClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ArcGISImageServiceLayer lyr = map1.Layers[0] as ArcGISImageServiceLayer;
                RenderingRule renderingRule = new RenderingRule();
                // Note: The .RasterFunctonName must use the text string 'Colormap'.
                renderingRule.RasterFunctionName = "Colormap";

                // Note: the .VariableName must use the text string 'Raster'.
                renderingRule.VariableName = "Raster";

                // Define a Dictionary object with String/Object pairs. The Strings that are used for the keys as 
                // Dictionary entries will need to have exact text strings depending on what raster function is being used.
                Dictionary<string, object> rasterParams = new Dictionary<string, object>();

                // Example #1: Create a random set of colors for the color map.
                // Note: The a key of 'ColormapName' takes the string 'Random'.

                rasterParams.Add("ColormapName", RasterParamCombo.SelectionBoxItem.ToString());
                // Add the rasterParms Dictionary as the RenderingRule's RasterFunctionArguments.
                renderingRule.RasterFunctionArguments = rasterParams;
                // Apply the user defined myRenderingRule to the ArcGISImageServiceLayer's .RenderingRule
                lyr.RenderingRule = renderingRule;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error making color map: " + ex.Message);
            }
        }
    }
}
