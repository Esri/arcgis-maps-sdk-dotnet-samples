using Esri.ArcGISRuntime.Tasks.Query;
using Microsoft.Phone.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Query Tasks</category>
	public partial class QueryOnly : PhoneApplicationPage
    {
        public QueryOnly()
        {
            InitializeComponent();
        }

        // Query states when Find button is pressed
        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            await doQuery();
        }

        // Query states when enter is pressed
        private async void StateNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // Check whether the key pressed was the Enter key
            {
                this.Focus(); // Put focus on the page to dismiss the keyboard
                await doQuery();
            }
        }

        private async Task doQuery()
        {
            // Clear previous results and show busy indicator
            ResultsItemsControl.ItemsSource = null;
            ProgressBar.Visibility = Visibility.Visible;

            // Create task to query for states
            QueryTask queryTask =
                new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));

            // Specify query parameters.  Query for the inptu text and include all fields in the results
            Query query = new Query(StateNameTextBox.Text);
            query.OutFields.Add("*");

            try
            {
                // Do the query and update the results
                var result = await queryTask.ExecuteAsync(query);
                ResultsItemsControl.ItemsSource = result.FeatureSet.Features;
            }
            catch (TaskCanceledException taskCanceledEx)
            {
                System.Diagnostics.Debug.WriteLine(taskCanceledEx.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // Hide busy indicator
            ProgressBar.Visibility = Visibility.Collapsed;
        }

    }
}