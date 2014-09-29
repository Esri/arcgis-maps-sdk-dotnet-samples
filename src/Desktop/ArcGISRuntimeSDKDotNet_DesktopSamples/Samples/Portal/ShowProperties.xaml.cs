﻿using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample illustrates a simple way of querying the Portal metadata/properties and retrieving featuredItems using the Portal API.
    /// </summary>
    /// <title>Show Properties</title>
    /// <category>Portal</category>
    public partial class ShowProperties : UserControl
    {
        /// <summary>Construct Show Portal Properties sample control</summary>
        public ShowProperties()
        {
            InitializeComponent();
        }

        private async void LoadPortalInfo_Click(object sender, RoutedEventArgs e)
        {
            PropertiesListBox.Items.Clear();
            if (String.IsNullOrEmpty(PortalUrltextbox.Text))
                return;

            await InitializePortalAsync(PortalUrltextbox.Text);
        }

        // Initializes the given portal and populates UI
        private async Task InitializePortalAsync(string portalUrl)
        {
            try
            {
                var portal = await ArcGISPortal.CreateAsync(new Uri(portalUrl));

                ArcGISPortalInfo portalInfo = portal.ArcGISPortalInfo;
                if (portalInfo == null)
                {
                    MessageBox.Show("Portal Information could not be retrieved");
                    return;
                }

                PropertiesListBox.Items.Add(string.Format("Current Version: {0}", portal.CurrentVersion));
                PropertiesListBox.Items.Add(string.Format("Access: {0}", portalInfo.Access));
                PropertiesListBox.Items.Add(string.Format("Host Name: {0}", portalInfo.PortalHostname));
                PropertiesListBox.Items.Add(string.Format("Name: {0}", portalInfo.PortalName));
                PropertiesListBox.Items.Add(string.Format("Mode: {0}", portalInfo.PortalMode));

                Basemap basemap = portalInfo.DefaultBasemap;

                PropertiesListBox.Items.Add(string.Format("Default BaseMap Title: {0}", basemap.Title));
                PropertiesListBox.Items.Add(string.Format("WebMap Layers ({0}):", basemap.Layers.Count));

                foreach (WebMapLayer webmapLayer in basemap.Layers)
                {
                    PropertiesListBox.Items.Add(webmapLayer.Url);
                }

                var portalgroup = await portalInfo.GetFeaturedGroupsAsync();
                PropertiesListBox.Items.Add("Groups:");

                ListBox listGroups = new ListBox();
                listGroups.ItemTemplate = layoutGrid.Resources["PortalGroupTemplate"] as DataTemplate;
                listGroups.ItemsSource = portalgroup;
                PropertiesListBox.Items.Add(listGroups);

                var result = await portalInfo.SearchFeaturedItemsAsync(new SearchParameters() { Limit = 15 });
                FeaturedMapsList.ItemsSource = result.Results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize" + ex.Message);
            }
        }
    }
}
