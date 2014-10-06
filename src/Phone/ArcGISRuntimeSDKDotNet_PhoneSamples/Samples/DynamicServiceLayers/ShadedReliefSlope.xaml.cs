using System;
using System.Collections.Generic;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class ShadedReliefSlope : Page
    {
        public ShadedReliefSlope()
        {
            this.InitializeComponent();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-13201378, 4359503, -13092435, 4412872, SpatialReferences.WebMercator));
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ArcGISImageServiceLayer imageLayer = mapView1.Map.Layers["ImageServiceLayer"] as ArcGISImageServiceLayer;
                RenderingRule renderingRule = new RenderingRule();
                Dictionary<string, object> rasterParams = new Dictionary<string, object>();

                if (SRRadioButton.IsChecked.Value)
                {
                    renderingRule.RasterFunctionName = "ShadedRelief";
                    renderingRule.VariableName = "Raster";

                    rasterParams.Add("Azimuth", string.IsNullOrEmpty(AzimuthTextBox.Text) ? 0 : double.Parse(AzimuthTextBox.Text));
                    rasterParams.Add("Altitude", string.IsNullOrEmpty(AltitudeTextBox.Text) ? 0 : double.Parse(AltitudeTextBox.Text));
                    rasterParams.Add("ZFactor", string.IsNullOrEmpty(ZFactorTextBox.Text) ? 0 : double.Parse(ZFactorTextBox.Text));

                    if (ColormapCheckBox.IsChecked.Value)
                        rasterParams.Add("Colormap", CreateColorMap());
                    else
                    {
                        renderingRule.RasterFunctionName = "Hillshade";
                        renderingRule.VariableName = "DEM";
                    }
                    renderingRule.RasterFunctionArguments = rasterParams;
                }
                else
                {
                    renderingRule.RasterFunctionName = "Slope";
                    renderingRule.VariableName = "DEM";

                    rasterParams.Add("ZFactor", string.IsNullOrEmpty(ZFactorTextBox.Text) ? 0 : double.Parse(ZFactorTextBox.Text));

                    renderingRule.RasterFunctionArguments = rasterParams;
                }

                imageLayer.RenderingRule = renderingRule;
            }
            catch (Exception)
            {
            }
        }

        private int[][] CreateColorMap()
        {
            int[][] sampleColormap = new int[][]{ 
        
            new int[]{0,175,240,233},
            new int[]{3,175,240,222},
            new int[]{7,177,242,212},
            new int[]{11,177,242,198},
            new int[]{15,176,245,183},
            new int[]{19,185,247,178},
            new int[]{23,200,247,178},
            new int[]{27,216,250,177},
            new int[]{31,232,252,179},
            new int[]{35,248,252,179},
            new int[]{39,238,245,162},
            new int[]{43,208,232,135},
            new int[]{47,172,217,111},
            new int[]{51,136,204,88},
            new int[]{55,97,189,66},
            new int[]{59,58,176,48},
            new int[]{63,32,161,43},
            new int[]{67,18,148,50},
            new int[]{71,5,133,58},
            new int[]{75,30,130,62},
            new int[]{79,62,138,59},
            new int[]{83,88,145,55},
            new int[]{87,112,153,50},
            new int[]{91,136,158,46},
            new int[]{95,162,166,41},
            new int[]{99,186,171,34},
            new int[]{103,212,178,25},
            new int[]{107,237,181,14},
            new int[]{111,247,174,2},
            new int[]{115,232,144,2},
            new int[]{119,219,118,2},
            new int[]{123,204,93,2},
            new int[]{127,191,71,2},
            new int[]{131,176,51,2},
            new int[]{135,163,34,2},
            new int[]{139,148,21,1},
            new int[]{143,135,8,1},
            new int[]{147,120,5,1},
            new int[]{151,117,14,2},
            new int[]{155,117,22,5},
            new int[]{159,115,26,6},
            new int[]{163,112,31,7},
            new int[]{167,112,36,8},
            new int[]{171,110,37,9},
            new int[]{175,107,41,11},
            new int[]{179,107,45,12},
            new int[]{183,105,48,14},
            new int[]{187,115,61,28},
            new int[]{191,122,72,40},
            new int[]{195,133,86,57},
            new int[]{199,140,99,73},
            new int[]{203,148,111,90},
            new int[]{207,153,125,109},
            new int[]{213,163,148,139},
            new int[]{217,168,163,160},
            new int[]{223,179,179,179},
            new int[]{227,189,189,189},
            new int[]{231,196,196,196},
            new int[]{235,207,204,207},
            new int[]{239,217,215,217},
            new int[]{243,224,222,224},
            new int[]{247,235,232,235},
            new int[]{251,245,242,245},
            new int[]{255,255,252,255}};
            return sampleColormap;
        }
    }
}
