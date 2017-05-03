﻿// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntimeXamarin.Managers;
using ArcGISRuntimeXamarin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin
{
    public partial class SampleListPage : ContentPage
    {
        private string _categoryName;
        private List<SampleModel> _listSampleItems;

        public SampleListPage(string name)
        {
            _categoryName = name;
            Initialize();

            InitializeComponent();

            Title = _categoryName;
        }

        void Initialize()
        {
            var sampleCategories = SampleManager.Current.GetSamplesAsTree();
            var category = sampleCategories.FirstOrDefault(x => x.Name == _categoryName) as TreeItem;

            List<object> listSubCategories = new List<object>();
            for (int i = 0; i < category.Items.Count; i++)
            {
                listSubCategories.Add((category.Items[i] as TreeItem).Items);
            }

            _listSampleItems = new List<SampleModel>();
            foreach (List<object> subCategoryItem in listSubCategories)
            {
                foreach (var sample in subCategoryItem)
                {
                    _listSampleItems.Add(sample as SampleModel);
                }
            }

            BindingContext = _listSampleItems;
        }

        async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var item = (SampleModel)e.Item;
                var sampleName = item.SampleName;
                var sampleNamespace = item.SampleNamespace;

                Type t = Type.GetType(sampleNamespace + "." + sampleName);

                //If Offline data is required for the sample to work download it 
                if (item.RequiresOfflineData)
                {
                    foreach (string id in item.DataItemIds)
                    {
                        //TODO - Add splash screen/progress bar
                        await DataManager.GetData(id, sampleName);
                    }
                }

                await Navigation.PushAsync((ContentPage)Activator.CreateInstance(t));

                // Call a function to clear existing credentials
                ClearCredentials();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(string.Format("Exception occurred on OnItemTapped. Exception = ", ex));
            }
        }

        private void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs
            var creds = Esri.ArcGISRuntime.Security.AuthenticationManager.Current.Credentials;
            for (var i = creds.Count() - 1; i >= 0; i--)
            {
                var c = creds.ElementAtOrDefault(i);
                if (c != null)
                {
                    Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveCredential(c);
                }
            }
        }
    }
}
