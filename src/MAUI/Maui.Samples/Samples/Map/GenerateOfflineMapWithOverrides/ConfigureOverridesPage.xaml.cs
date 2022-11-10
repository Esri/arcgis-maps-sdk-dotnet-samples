using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Offline;

namespace ArcGISMapsSDKMaui.Samples.GenerateOfflineMapWithOverrides
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigureOverridesPage : ContentPage
    {
        private Esri.ArcGISRuntime.Mapping.Map _map;
        private Envelope _areaOfInterest = new Envelope(-88.1541, 41.7690, -88.1471, 41.7720, SpatialReferences.Wgs84);
        private GenerateOfflineMapParameterOverrides _overrides;

        public ConfigureOverridesPage()
        {
            InitializeComponent();
        }

        public ConfigureOverridesPage(GenerateOfflineMapParameterOverrides overrides, Esri.ArcGISRuntime.Mapping.Map map)
        {
            InitializeComponent();

            // Hold a reference to the map & overrides.
            _map = map;
            _overrides = overrides;
        }

        #region overrides

        private void ConfigureTileLayerOverrides(GenerateOfflineMapParameterOverrides overrides)
        {
            // Create a parameter key for the first basemap layer.
            OfflineMapParametersKey basemapKey = new OfflineMapParametersKey(_map.Basemap.BaseLayers.First());

            // Get the export tile cache parameters for the layer key.
            ExportTileCacheParameters basemapParams = overrides.ExportTileCacheParameters[basemapKey];

            // Clear the existing level IDs.
            basemapParams.LevelIds.Clear();

            // Get the min and max scale from the UI.
            int minScale = (int)MinScaleEntry.Value;
            int maxScale = (int)MaxScaleEntry.Value;

            // Re-add selected scales.
            for (int i = minScale; i < maxScale; i++)
            {
                basemapParams.LevelIds.Add(i);
            }

            // Expand the area of interest based on the specified buffer distance.
            int bufferDistance = (int)ExtentBufferEntry.Value;
            basemapParams.AreaOfInterest = GeometryEngine.BufferGeodetic(_areaOfInterest, bufferDistance, LinearUnits.Meters);
        }

        private void ConfigureLayerExclusion(GenerateOfflineMapParameterOverrides overrides)
        {
            // Apply layer exclusions as specified in the UI.
            if (ServiceConnCheckbox.IsToggled == false)
            {
                ExcludeLayerByName("Service Connection", overrides);
            }

            if (SysValvesLayerCheckbox.IsToggled == false)
            {
                ExcludeLayerByName("System Valve", overrides);
            }
        }

        private void CropWaterPipes(GenerateOfflineMapParameterOverrides overrides)
        {
            if (CropLayerCheckbox.IsToggled)
            {
                // Get the ID of the water pipes layer.
                long targetLayerId = GetServiceLayerId(GetLayerByName("Main"));

                // For each layer option.
                foreach (GenerateLayerOption layerOption in GetAllLayerOptions(overrides))
                {
                    // If the option's LayerId matches the selected layer's ID.
                    if (layerOption.LayerId == targetLayerId)
                    {
                        layerOption.UseGeometry = true;
                    }
                }
            }
        }

        private void ApplyFeatureFilter(GenerateOfflineMapParameterOverrides overrides)
        {
            // For each layer option.
            foreach (GenerateLayerOption option in GetAllLayerOptions(overrides))
            {
                // If the option's LayerId matches the selected layer's ID.
                if (option.LayerId == GetServiceLayerId(GetLayerByName("Hydrant")))
                {
                    // Apply the where clause.
                    option.WhereClause = "FLOW >= " + (int)FlowRateFilterEntry.Value;
                    // Configure the option to use the where clause.
                    option.QueryOption = GenerateLayerQueryOption.UseFilter;
                }
            }
        }

        private IList<GenerateLayerOption> GetAllLayerOptions(GenerateOfflineMapParameterOverrides overrides)
        {
            // Find the first feature layer.
            FeatureLayer targetLayer = _map.OperationalLayers.OfType<FeatureLayer>().First();

            // Get the key for the layer.
            OfflineMapParametersKey layerKey = new OfflineMapParametersKey(targetLayer);

            // Use that key to get the generate options for the layer.
            GenerateGeodatabaseParameters generateParams = overrides.GenerateGeodatabaseParameters[layerKey];

            // Return the layer options.
            return generateParams.LayerOptions;
        }

        private void ExcludeLayerByName(string layerName, GenerateOfflineMapParameterOverrides overrides)
        {
            // Get the feature layer with the specified name.
            FeatureLayer targetLayer = GetLayerByName(layerName);

            // Get the layer's ID.
            long targetLayerId = GetServiceLayerId(targetLayer);

            // Create a layer key for the selected layer.
            OfflineMapParametersKey layerKey = new OfflineMapParametersKey(targetLayer);

            // Get the parameters for the layer.
            GenerateGeodatabaseParameters generateParams = overrides.GenerateGeodatabaseParameters[layerKey];

            // Get the layer options for the layer.
            IList<GenerateLayerOption> layerOptions = generateParams.LayerOptions;

            // Find the layer option matching the ID.
            GenerateLayerOption targetLayerOption = layerOptions.First(layer => layer.LayerId == targetLayerId);

            // Remove the layer option.
            layerOptions.Remove(targetLayerOption);
        }

        private FeatureLayer GetLayerByName(string layerName)
        {
            // Get the first map in the operational layers collection that is a feature layer with name matching layerName
            return _map.OperationalLayers.OfType<FeatureLayer>().First(layer => layer.Name == layerName);
        }

        private long GetServiceLayerId(FeatureLayer layer)
        {
            // Find the service feature table for the layer; this assumes the layer is backed by a service feature table.
            ServiceFeatureTable serviceTable = (ServiceFeatureTable)layer.FeatureTable;

            // Return the layer ID.
            return serviceTable.LayerInfo.ServiceLayerId;
        }

        #endregion overrides

        private void TakeMapOffline_Clicked(object sender, EventArgs e)
        {
            // Configure the overrides using helper methods.
            ConfigureTileLayerOverrides(_overrides);
            ConfigureLayerExclusion(_overrides);
            CropWaterPipes(_overrides);
            ApplyFeatureFilter(_overrides);

            // The main sample page continues when OnDisappearing is called.
            // OnDisappearing is called by Xamarin.ArcGISRuntimeMaui when navigation away from this page happens.
            Navigation.PopModalAsync(true);
        }
    }
}