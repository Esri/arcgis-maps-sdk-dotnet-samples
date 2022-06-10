using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Offline;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntime.Samples.GenerateOfflineMapWithOverrides
{
    public class ParameterOverrideFragment : DialogFragment
    {
        // Hold references to the overrides, map, and area of interest.
        private GenerateOfflineMapParameterOverrides _overrides;
        private Map _map;
        private Envelope _areaOfInterest = new Envelope(-88.1541, 41.7690, -88.1471, 41.7720, SpatialReferences.Wgs84);

        // Hold references to the UI controls.
        private SeekBar _minScaleBar;
        private SeekBar _maxScaleBar;
        private SeekBar _extentBufferBar;
        private SeekBar _maxFlowBar;
        private TextView _minScaleValueView;
        private TextView _maxScaleValueView;
        private TextView _extentBufferValueView;
        private TextView _maxFlowValueView;
        private CheckBox _serviceConnCheckbox;
        private CheckBox _sysValveCheckbox;
        private CheckBox _cropLayerCheckBox;

        public ParameterOverrideFragment()
        {
        }

        public ParameterOverrideFragment(GenerateOfflineMapParameterOverrides overrides, Map map)
        {
            _overrides = overrides;
            _map = map;
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
            int minScale = _minScaleBar.Progress;
            int maxScale =  _maxScaleBar.Progress;

            // Re-add selected scales.
            for (int i = minScale; i < maxScale; i++)
            {
                basemapParams.LevelIds.Add(i);
            }

            // Expand the area of interest based on the specified buffer distance.
            int bufferDistance = _extentBufferBar.Progress;
            basemapParams.AreaOfInterest = GeometryEngine.BufferGeodetic(_areaOfInterest, bufferDistance, LinearUnits.Meters);
        }

        private void ConfigureLayerExclusion(GenerateOfflineMapParameterOverrides overrides)
        {
            // Apply layer exclusions as specified in the UI.
            if (!_serviceConnCheckbox.Checked)
            {
                ExcludeLayerByName("Service Connection", overrides);
            }

            if (!_sysValveCheckbox.Checked)
            {
                ExcludeLayerByName("System Valve", overrides);
            }
        }

        private void CropWaterPipes(GenerateOfflineMapParameterOverrides overrides)
        {
            if (_cropLayerCheckBox.Checked)
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
                    option.WhereClause = $"FLOW >= {_maxFlowBar.Progress}";
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
            ServiceFeatureTable serviceTable = (ServiceFeatureTable) layer.FeatureTable;

            // Return the layer ID.
            return serviceTable.LayerInfo.ServiceLayerId;
        }

        #endregion overrides

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            // Load the layout from the XML file.
            LayoutInflater inflater = (LayoutInflater)Activity.GetSystemService(Context.LayoutInflaterService);
            View layout = inflater.Inflate(Resource.Layout.OverrideParametersDialog, null);

            // Update the references to the UI controls.
            _minScaleBar = layout.FindViewById<SeekBar>(Resource.Id.minScaleSeekBar);
            _maxScaleBar = layout.FindViewById<SeekBar>(Resource.Id.maxScaleSeekBar);
            _extentBufferBar = layout.FindViewById<SeekBar>(Resource.Id.extentBufferDistanceSeekBar);
            _maxFlowBar = layout.FindViewById<SeekBar>(Resource.Id.minHydrantFlowRateSeekBar);

            _minScaleBar.ProgressChanged += BarValueChanged;
            _maxScaleBar.ProgressChanged += BarValueChanged;
            _extentBufferBar.ProgressChanged += BarValueChanged;
            _maxFlowBar.ProgressChanged += BarValueChanged;

            _minScaleValueView = layout.FindViewById<TextView>(Resource.Id.currMinScaleTextView);
            _maxScaleValueView = layout.FindViewById<TextView>(Resource.Id.currMaxScaleTextview);
            _extentBufferValueView = layout.FindViewById<TextView>(Resource.Id.currExtentBufferDistanceTextView);
            _maxFlowValueView = layout.FindViewById<TextView>(Resource.Id.currMinHydrantFlowRateTextView);

            _serviceConnCheckbox = layout.FindViewById<CheckBox>(Resource.Id.serviceConnectionsCheckBox);
            _sysValveCheckbox = layout.FindViewById<CheckBox>(Resource.Id.systemValvesCheckBox);
            _cropLayerCheckBox = layout.FindViewById<CheckBox>(Resource.Id.waterPipesCheckBox);

            // Show the dialog.
            var builder = new AlertDialog.Builder(Activity).SetView(layout).SetTitle("Override parameters");
            builder.SetPositiveButton("Take map offline", TakeMapOffline_Clicked);
            return builder.Create();
        }

        private void TakeMapOffline_Clicked(object sender, DialogClickEventArgs e)
        {
            // Configure the overrides using helper methods.
            ConfigureTileLayerOverrides(_overrides);
            ConfigureLayerExclusion(_overrides);
            CropWaterPipes(_overrides);
            ApplyFeatureFilter(_overrides);

            // Dismiss the dialog.
            Dismiss();

            // Raise the event.
            FinishedConfiguring?.Invoke(this, null);
        }

        private void BarValueChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (sender == _minScaleBar)
            {
                _minScaleValueView.Text = _minScaleBar.Progress.ToString();
            }
            else if (sender == _maxScaleBar)
            {
                _maxScaleValueView.Text = _maxScaleBar.Progress.ToString();
            }
            else if (sender == _extentBufferBar)
            {
                _extentBufferValueView.Text = $"{_extentBufferBar.Progress}m";
            }
            else if (sender == _maxFlowBar)
            {
                _maxFlowValueView.Text = $"{_maxFlowBar.Progress} GPM";
            }
        }

        public event EventHandler FinishedConfiguring;
    }
}