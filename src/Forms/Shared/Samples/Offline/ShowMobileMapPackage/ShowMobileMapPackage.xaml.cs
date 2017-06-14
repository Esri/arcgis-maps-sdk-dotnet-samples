// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ShowMobileMapPackage
{
    public partial class ShowMobileMapPackage : ContentPage
    {

        public ShowMobileMapPackage()
        {
            InitializeComponent();

            Title = "Show mobile map package metadata";
            
        }

        private async void OnDownloadDataClicked(object sender, EventArgs e)
        {
            try
            {
                if (SampleManager.Current.SelectedSample.DataItemIds != null)
                {
                    foreach (string id in SampleManager.Current.SelectedSample.DataItemIds)
                    {
                        await DataManager.GetData(id, SampleManager.Current.SelectedSample.Name);
                    }
                }
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}