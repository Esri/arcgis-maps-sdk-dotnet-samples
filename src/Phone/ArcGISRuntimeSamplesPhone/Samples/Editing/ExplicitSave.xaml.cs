using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// Demonstrates how to accumulated edits can be saved or canceled altogether.
    /// </summary>
    /// <title>Explicit Save</title>
    /// <category>Editing</category>
    public partial class ExplicitSave : Page
    {
        public ExplicitSave()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Adds new feature on tap.
        /// </summary>
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Map.Layers["Notes"] as FeatureLayer;
            var table = layer.FeatureTable;
            string message = null;
            try
            {
                var mapPoint = await MyMapView.Editor.RequestPointAsync();
                var feature = new GeodatabaseFeature(table.Schema)
                {
                    Geometry = mapPoint
                };
                await table.AddAsync(feature);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Saves accumulated edits.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Map.Layers["Notes"] as FeatureLayer;
            var table = (ArcGISFeatureTable)layer.FeatureTable;
            string message = null;
            try
            {
                if (!table.HasEdits)
                    return;
                if (table is ServiceFeatureTable)
                {
                    var serviceTable = (ServiceFeatureTable)table;
                    // Pushes accumulated edits back to the server.
                    var saveResult = await serviceTable.ApplyEditsAsync();
                    if (saveResult != null && saveResult.AddResults != null
                        && saveResult.AddResults.All(r => r.Error == null && r.Success))
                        message = string.Format("Saved {0} features", saveResult.AddResults.Count);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Cancels accumulated edits.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Map.Layers["Notes"] as FeatureLayer;
            var table = (ArcGISFeatureTable)layer.FeatureTable;
            if (table.HasEdits)
                table.ClearEdits();
        }
    }
}