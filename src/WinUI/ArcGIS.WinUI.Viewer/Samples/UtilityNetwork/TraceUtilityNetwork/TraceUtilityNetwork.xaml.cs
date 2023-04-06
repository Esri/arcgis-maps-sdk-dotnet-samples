// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.TraceUtilityNetwork
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Trace utility network",
        category: "Utility network",
        description: "Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.",
        instructions: "Select a trace configuration from the menu. Tap on features after pressing 'Add starting point' button. Delete unwanted starting points by pressing the corresponding trash icon. Press 'Run trace' to initiate a trace on the network. View the results, then 'Clear results' to start over.",
        tags: new[] { "condition barriers", "downstream trace", "network analysis", "subnetwork trace", "trace configuration", "traversability", "upstream trace", "utility network", "validate consistency" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class TraceUtilityNetwork : UserControl
    {
        private const string WebmapURL = "https://www.arcgis.com/home/item.html?id=471eb0bf37074b1fbb972b1da70fb310";

        public TraceUtilityNetwork()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Using public credentials from https://developers.arcgis.com/javascript/latest/sample-code/widgets-untrace/
                TokenCredential portal1Credential = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri("https://sampleserver7.arcgisonline.com/portal/sharing/rest"), "viewer01", "I68VGU^nMurF");
                AuthenticationManager.Current.AddCredential(portal1Credential);

                MyMapView.Map = new Map(new Uri(WebmapURL));
            }
            catch (Exception ex)
            {
                new MessageDialog2($"Initializing sample failed: {ex.Message}", ex.GetType().Name);
            }
        }
    }
}