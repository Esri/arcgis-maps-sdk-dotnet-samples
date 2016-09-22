// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Windows.UI.Core;

namespace ArcGISRuntimeXamarin.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new ArcGISRuntimeXamarin.App());

            // Workaround : If ContentPage that is shown contains MapView, first backwards navigation removes the MapView
            // and doesn't do the full navigation. As a workaround, check if there are 3 items in a navigation stack and 
            // manually remove the sample page. 
            // Items in the stack are following
            // [0] : CategoriesPage
            // [1] : SampleListPage
            // [2] : Selected sample 
            // [X] : Possible other pages
            SystemNavigationManager.GetForCurrentView().BackRequested += ContentPageRenderer_BackRequested;
        }

        private void ContentPageRenderer_BackRequested(object sender, BackRequestedEventArgs e)
        {
            var x = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack;
            // If we are on Samples page remove it from the stack.
            if (x.Count == 3)
            {
                Xamarin.Forms.Application.Current.MainPage.Navigation.PopAsync();
            }
        }
    }
}
