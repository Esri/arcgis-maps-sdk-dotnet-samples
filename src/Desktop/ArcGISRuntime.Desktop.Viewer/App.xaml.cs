//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using ArcGISRuntime.Desktop.Viewer.Managers;
using ArcGISRuntime.Samples.Models;
using System.Windows;

namespace ArcGISRuntime.Desktop.Viewer
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var selectedLanguage = Language.CSharp;

            // Check application parameters:
            // parameter definitions:
            //     /vb = launch application using VBNet samples, defaults to C#
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/vb")
                {
                    selectedLanguage = Language.VBNet;
                }
            }

            ApplicationManager.Current.Initialize(selectedLanguage);
        }
    }
}
