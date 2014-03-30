using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Query Tasks</category>
	public partial class Statistics : PhoneApplicationPage
    {
        public Statistics()
        {
            InitializeComponent();

            // Set the data context to the page instance to allow for binding to the page's properties
            // in its XAML
            DataContext = this;
        }

        // Get statistics when Calculate button is pressed
        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous results and show busy indicator
            Results = null;
            BusyVisibility = Visibility.Visible;

            // Do stats calculation
            Results = await calculateStatistics();

            // Hide busy indicator
            BusyVisibility = Visibility.Collapsed;
        }

        // Performs statistics calculation and returns results
        private async Task<IEnumerable<Graphic>> calculateStatistics()
        {
            IEnumerable<Graphic> results = null;

            // Initialize the base of the URL pointing to the service used to calculate stats
            var serviceUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/";

            // Check which feature set statistics are to be calculated for
            string groupByField = "";
            if (StatisticsSet == "States by Region")
            {
                serviceUrl += "2"; // Append ID of states layer to URL
                groupByField = "sub_region"; // Name of field for grouping by region
            }
            else if (StatisticsSet == "Counties by State")
            {
                serviceUrl += "3"; // Append ID of counties layer to URL
                groupByField = "state_name"; // Name of field for grouping by state
            }

            // Initialize query task with service URL
            QueryTask queryTask =
                new QueryTask(new Uri(serviceUrl));

            // Initialize query parameters
            Query query = new Query("1=1")
            {
                GroupByFieldsForStatistics = new List<string> { groupByField }, // Field results will be grouped by
                OutStatistics = new List<OutStatistic> { 
                    new OutStatistic(){
                        OnStatisticField = "pop2000", // Population field - used for calculating statistics
                        OutStatisticFieldName = "pop_summary", // Name of field containing results
                        StatisticType = Statistic // The statistic to calculate
                    },
                    new OutStatistic(){
                        OnStatisticField = "pop00_sqmi", // Population density field
                        OutStatisticFieldName = "pop_density_summary",
                        StatisticType = Statistic
                    },
                    new OutStatistic(){
                        OnStatisticField = groupByField, // Field grouping was performed on
                        OutStatisticFieldName = "count",
                        StatisticType = StatisticType.Count
                    }
                 }
            };
            try
            {
                // Do the query
                var result = await queryTask.ExecuteAsync(query);

                // Check whether results were returned
                if (result.FeatureSet.Features != null && result.FeatureSet.Features.Count > 0)
                {
                    // Copy the field grouping was done on - region or state in this case - to a field called
                    // "name."  This is done so the view (i.e. XAML) can bind to one field regardless of what
                    // features stats were calculated for
                    foreach (Graphic g in result.FeatureSet.Features)
                        g.Attributes.Add("name", g.Attributes[groupByField]);

                    // Sort the results by name
                    results = result.FeatureSet.Features.OrderBy(gr => gr.Attributes["name"]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return results;
        }

        #region Bindable Properties - Statistic, Results, StatisticsSet, BusyVisibility

        #region Statistic
        /// <summary>
        /// Identifies the <see cref="Statistic"/> dependency property
        /// </summary>
        private static readonly DependencyProperty StatisticProperty = DependencyProperty.Register(
            "Statistic", typeof(StatisticType), typeof(Statistics), null);

        /// <summary>
        /// Gets or sets the statistic to calculate
        /// </summary>
        public StatisticType Statistic
        {
            get { return (StatisticType)GetValue(StatisticProperty); }
            set { SetValue(StatisticProperty, value); }
        }
        #endregion

        #region Results
        /// <summary>
        /// Identifies the <see cref="Results"/> dependency property
        /// </summary>
        private static readonly DependencyProperty ResultsProperty = DependencyProperty.Register(
            "Results", typeof(IEnumerable<Graphic>), typeof(Statistics), null);

        /// <summary>
        /// Gets the results from the most recent statistics calculation
        /// </summary>
        public IEnumerable<Graphic> Results
        {
            get { return GetValue(ResultsProperty) as IEnumerable<Graphic>; }
            private set { SetValue(ResultsProperty, value); }
        }
        #endregion

        #region StatisticsSet
        /// <summary>
        /// Identifies the <see cref="StatisticsSet"/> dependency property
        /// </summary>
        private static readonly DependencyProperty StatisticsSetProperty = DependencyProperty.Register(
            "StatisticsSet", typeof(string), typeof(Statistics), new PropertyMetadata("States by Region"));

        /// <summary>
        /// Gets or sets the set of features to calculate statistics for
        /// </summary>
        public string StatisticsSet
        {
            get { return GetValue(StatisticsSetProperty) as string; }
            set { SetValue(StatisticsSetProperty, value); }
        }
        #endregion

        #region BusyVisibility
        /// <summary>
        /// Identifies the <see cref="BusyVisibility"/> dependency property
        /// </summary>
        private static readonly DependencyProperty BusyVisibilityProperty = DependencyProperty.Register(
            "BusyVisibility", typeof(Visibility), typeof(Statistics),
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets whether an operation is currently in progress, expressed as a 
        /// <see cref="System.Windows.Visibility">Visibility</see>
        /// </summary>
        public Visibility BusyVisibility
        {
            get { return (Visibility)GetValue(BusyVisibilityProperty); }
            private set { SetValue(BusyVisibilityProperty, value); }
        }
        #endregion

        #endregion

        /// <summary>
        /// Gets all available <see cref="StatisticType">StatisticTypes</see>
        /// </summary>
        public IEnumerable<StatisticType> AllStatistics
        {
            get { return Enum.GetValues(typeof(StatisticType)).Cast<StatisticType>(); }
        }
    }
}