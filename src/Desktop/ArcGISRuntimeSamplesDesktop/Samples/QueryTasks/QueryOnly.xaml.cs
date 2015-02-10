using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to use a QueryTask to query a map service without a map.
	/// </summary>
	/// <title>Query Only</title>
	/// <category>Tasks</category>
	/// <subcategory>Query</subcategory>
	public partial class QueryOnly : UserControl
	{
		public QueryOnly()
		{
			InitializeComponent();
		}

		private async void QueryButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await RunQuery();
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
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
				MessageBox.Show(ex.Message, "Error");
			}
		}
	}
}
