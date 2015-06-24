using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Symbology;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop.DynamicLayers
{
    /// <summary>
    /// This sample demonstrates the dynamic layers capability to add Shapefiles and Raster datasets to the map. Single or multiple Shapefiles or Rasters can be selected in the open file dialog from within a folder. These files will be added to a local map service via the dynamic layers capability available in the API. This local map service is then used as the basis for an ArcGISDynamicMapServiceLayer which is added to the map. It would be possible to extend the sample to allows users to specify the symbol/renderer for any shapefiles added. Raster symbology is predetermined by the raster itself.
    /// </summary>
    /// <title>Dynamic Layer Add Data</title>
    /// <category>Layers</category>
    /// <subcategory>Dynamic Service Layers</subcategory>
    /// <requiresLocalServer>true</requiresLocalServer>
    public partial class DynamicLayerAddData : UserControl
    {
        private const string _emptyMapPackage = @"..\..\..\samples-data\maps\water-distribution-network.mpk";
        private Random _random = new Random();

        /// <summary>Construct DynamicLayerAddData user control</summary>
        public DynamicLayerAddData()
        {
            InitializeComponent();
            progress.Visibility = Visibility.Collapsed;
        }

        // Add shapefiles
        private async void AddShapefileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Shapefiles (*.shp)|*.shp";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    progress.Visibility = Visibility.Visible;
                    List<string> fileNames = new List<string>();
                    foreach (var item in openFileDialog.SafeFileNames)
                    {
                        fileNames.Add(Path.GetFileNameWithoutExtension(item));
                    }

                    // Call the add dataset method with workspace type, parent directory path and file names (without extensions)
                    var dynLayer = await AddFileDatasetToDynamicMapServiceLayer(WorkspaceFactoryType.Shapefile,
                        Path.GetDirectoryName(openFileDialog.FileName), fileNames);

                    // Add the dynamic map service layer to the map
                    if (dynLayer != null)
                    {
                        dynLayer.DisplayName = dynLayer.DynamicLayerInfos[0].Name;
                        MyMapView.Map.Layers.Add(dynLayer);
                        progress.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        // Add raster datasets
        private async void AddRasterButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp,*.png,*.sid,*.tif)|*.bmp;*.png;*.sid;*.tif;";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    progress.Visibility = Visibility.Visible;
                    // Call the add dataset method with workspace type, parent directory path and actual file names
                    var dynLayer = await AddFileDatasetToDynamicMapServiceLayer(WorkspaceFactoryType.Raster,
                        Path.GetDirectoryName(openFileDialog.FileName), new List<string>(openFileDialog.SafeFileNames));

                    // Add the dynamic map service layer to the map
                    if (dynLayer != null)
                    {
                        dynLayer.DisplayName = dynLayer.DynamicLayerInfos[0].Name;
                        MyMapView.Map.Layers.Add(dynLayer);
                        progress.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private async Task<ArcGISDynamicMapServiceLayer> AddFileDatasetToDynamicMapServiceLayer(WorkspaceFactoryType workspaceType, string directoryPath, List<string> fileNames)
        {
            try
            {
                // Create a new WorkspaceInfo object with a unique ID.
                string uniqueId = Guid.NewGuid().ToString();
                WorkspaceInfo workspaceInfo = new WorkspaceInfo(uniqueId, workspaceType, "DATABASE=" + directoryPath);

                // Create and initialize a new LocalMapService instance.
                LocalMapService localMapService = new LocalMapService(_emptyMapPackage) { EnableDynamicLayers = true };
                localMapService.DynamicWorkspaces.Add(workspaceInfo);
                await localMapService.StartAsync();

                // Create and initialize new ArcGISLocalDynamicMapServiceLayer over the local service.
                var dynLayer = new ArcGISDynamicMapServiceLayer()
                {
                    ID = "Workspace: " + (new DirectoryInfo(directoryPath)).Name,
                    ServiceUri = localMapService.UrlMapService
                };
                await dynLayer.InitializeAsync();

                // Create a DynamicLayerInfoCollection to hold the new datasets as "dynamic layers".
                DynamicLayerInfoCollection dynamicLayerInfoCollection = new DynamicLayerInfoCollection();
                dynLayer.DynamicLayerInfos = dynamicLayerInfoCollection;

                // Create a LayerDrawingOptionsCollection to specify the symbology for each layer.
                LayerDrawingOptionCollection layerDrawingOptionsCollection = new LayerDrawingOptionCollection();
                dynLayer.LayerDrawingOptions = layerDrawingOptionsCollection;

                // Iterate over each of the selected files in the workspace.
                int counter = 0;
                foreach (string fileName in fileNames)
                {
                    // Create a new DynamicLayerInfo (to make changes to existing map service layers use the CreateDynamicLayerInfosFromLayerInfos() method.
                    DynamicLayerInfo dynamicLayerInfo = new DynamicLayerInfo { ID = counter, Name = "Dataset: " + fileName };

                    // Create a DataSource object to represent the physical datasource implementation (table or raster) which will become the DataSource 
                    // property of a new LayerDataSource in the map service. Other supported datasource types are JoinDataSource and QueryDataSource.
                    DataSource dataSource = null;

                    // If the workspace type is Raster create a new RasterDataSource.
                    if (workspaceInfo.FactoryType == WorkspaceFactoryType.Raster)
                    {
                        // Create a new RasterDataSource object
                        dataSource = new RasterDataSource
                        {
                            // Match the DataSourceName to the physical filename on disk (including extension).
                            DataSourceName = fileName,

                            // Provide the WorkspaceID (the unique workspace identifier created earlier). A LocalMapService may have multiple dynamic workspaces.
                            WorkspaceID = workspaceInfo.Id
                        };
                    }
                    else
                    {
                        // Else if the workspace is not Raster create a new TableDataSource
                        dataSource = new TableDataSource
                        {
                            // Match the DataSourceName to the physical filename on disk (excluding extension).
                            DataSourceName = fileName,

                            // Provide the WorkspaceID (the unique workspace identifier created earlier). A LocalMapService may have multiple dynamic workspaces.
                            WorkspaceID = workspaceInfo.Id
                        };
                    }

                    // Set the Source property of the DynamicLayerInfo object.
                    dynamicLayerInfo.Source = new LayerDataSource { DataSource = dataSource };

                    // Add the new DynamicLayerInfo object to the collection.
                    dynamicLayerInfoCollection.Add(dynamicLayerInfo);

                    // Create a new LayerDrawingOptions object to hold the renderer information.
                    var layerDrawOpt = new LayerDrawingOptions()
                    {
                        // Match up the LayerID to the ID of the layer within the service.
                        LayerID = counter,
                    };

                    // Use the GetDetails method which now supports dynamic data sources to determine the geometry type of the new datasource.
                    var featureLayerInfo = await dynLayer.GetDetailsAsync(dynamicLayerInfo.ID);

                    switch (featureLayerInfo.GeometryType)
                    {
                        case GeometryType.Envelope:
                            layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleFillSymbol() { Color = GetRandomColor(), Outline = new SimpleLineSymbol() { Color = GetRandomColor() } } };
                            break;
                        case GeometryType.Multipoint:
                            layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleMarkerSymbol() { Color = GetRandomColor(), Size = 8 } };
                            break;
                        case GeometryType.Point:
                            layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleMarkerSymbol() { Color = GetRandomColor(), Size = 8 } };
                            break;
                        case GeometryType.Polygon:
                            layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleFillSymbol() { Color = GetRandomColor(), Outline = new SimpleLineSymbol() { Color = GetRandomColor() } } };
                            break;
                        case GeometryType.Polyline:
                            layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleLineSymbol() { Color = GetRandomColor() } };
                            break;
                        default:
                            break;
                    }

                    // Set the LayerDrawingOptions property on the local dynamic map service layer (the LayerID property ties this to the DynamicLayerInfo object).
                    layerDrawingOptionsCollection.Add(layerDrawOpt);

                    counter++;
                }

                return dynLayer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // Utility function: Generate a random System.Windows.Media.Color
        private Color GetRandomColor()
        {
            var colorBytes = new byte[3];
            _random.NextBytes(colorBytes);
            return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
        }
    }
}
