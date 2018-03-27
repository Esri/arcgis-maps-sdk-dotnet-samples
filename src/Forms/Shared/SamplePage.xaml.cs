// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArcGISRuntime
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplePage
    {
        public SamplePage()
        {
            InitializeComponent();
        }

        public SamplePage(ContentPage sample, SampleInfo sampleInfo) : this()
        {
            // Update the binding context - this is important for the description tab.
            BindingContext = sampleInfo;

            // Update the content - this displays the sample.
            SampleContentPage.Content = sample.Content;

            // Because the sample control isn't navigated to (its content is displayed directly),
            //    navigation won't work from within the sample until the parent is manually set.
            sample.Parent = this;

            // Set the title. If the sample control didn't 
            // define the title, use the name from the sample metadata.
            if (!string.IsNullOrWhiteSpace(sample.Title))
            {
                Title = sample.Title;
            }
            else
            {
                Title = sampleInfo.SampleName;
            }

            // Only show the instructions heading if there are any instructions.
            if (!string.IsNullOrWhiteSpace(sampleInfo.Instructions))
            {
                InstructionLabel.IsVisible = true;
            }
        }
    }
}