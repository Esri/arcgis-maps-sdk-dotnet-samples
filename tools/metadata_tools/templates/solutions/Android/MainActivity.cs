// Copyright $$current_year$$ Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.OS;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Attributes;
using ArcGISRuntime.Samples.Shared.Models;
using System;
using System.Linq;
using System.Reflection;

namespace ArcGISRuntime
{
    [Activity(Label = "ArcGIS Runtime SDK for .NET Samples", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get the sample type.
            Type sampleType = GetType().GetTypeInfo().Assembly
                .GetTypes().First(type => type.GetTypeInfo().GetCustomAttributes().OfType<SampleAttribute>().Any());

            // Populate the metadata for the sample.
            SampleInfo sample = new SampleInfo(sampleType);

            // Download offline data if necessary.
            if (sample.OfflineDataItems != null)
            {
                await DataManager.EnsureSampleDataPresent(sample);
            }

            var newActivity = new Intent(this, sample.SampleType);

            StartActivity(newActivity);
        }
    }
}