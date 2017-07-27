
' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace OpenMobileMap
    Partial Public Class OpenMobileMapVB
        ' Hold a reference to a Mobile Map Package
        Private MyMapPackage As MobileMapPackage

        Public Sub New()
            InitializeComponent()

            ' Execute initialization 
            Initialize()
        End Sub

        Private Async Sub Initialize()
            ' Load the Mobile Map Package
            '     File Is located in Resources/MobileMapPackages/Yellowstone.mmpk
            '     Build Action Is Content; Copy if newer
            MyMapPackage = Await MobileMapPackage.OpenAsync("ArcGISRuntime.UWP.Samples\\Resources\\MobileMapPackages\\Yellowstone.mmpk")

            ' Check that there Is at least one map
            If MyMapPackage.Maps.Count > 0 Then
                ' Display the first map in the package
                MyMapView.Map = MyMapPackage.Maps.First()
            End If
        End Sub
    End Class
End Namespace
