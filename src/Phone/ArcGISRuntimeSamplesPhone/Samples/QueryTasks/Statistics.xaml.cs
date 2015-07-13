using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to use a QueryTask to get statistics from a map service.
	/// </summary>
	/// <title>Statistics</title>
	/// <category>Query Tasks</category>
	public sealed partial class Statistics : Page
    {
        public Statistics()
        {
            this.InitializeComponent();

            RunQuery();
        }

        private async void RunQuery()
        {
            QueryTask queryTask =
                new QueryTask(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2"));

            Query query = new Query("1=1")
             {
                 GroupByFieldsForStatistics = new List<string> { "sub_region" },
                 OutStatistics = new List<OutStatistic> { 
                    new OutStatistic(){
                        OnStatisticField = "pop2000",
                        OutStatisticFieldName = "subregionpopulation",
                        StatisticType = StatisticType.Sum
                    },
                    new OutStatistic(){
                        OnStatisticField = "sub_region",
                        OutStatisticFieldName = "numberofstates",
                        StatisticType = StatisticType.Count
                    }
                 }
             };
            try
            {
                var result = await queryTask.ExecuteAsync(query);
                if (result.FeatureSet.Features != null && result.FeatureSet.Features.Count > 0)
                {
                    ResultGrid.ItemsSource = result.FeatureSet.Features;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // No format provided.
            if (parameter == null)
            {
                return value;
            }


            return String.Format((String)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

}