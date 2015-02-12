using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates how to use a QueryTask to query a map service without a map.
    /// </summary>
    /// <title>Query Only</title>
    /// <category>Query Tasks</category>
    public sealed partial class QueryOnly : Page
    {
        public QueryOnly()
        {
            this.InitializeComponent();
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await RunQuery();
            }
            catch (System.Exception ex)
            {
                var _x = new Windows.UI.Popups.MessageDialog(ex.Message, "Error").ShowAsync();
            }
        }

        private async Task RunQuery()
        {
            try
            {
                QueryTask queryTask = new QueryTask(
                    new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));

                Query query = new Query(StateNameTextBox.Text);
                query.OutFields.Add("*");

                var result = await queryTask.ExecuteAsync(query);
                itemListView.ItemsSource = result.FeatureSet.Features;
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
