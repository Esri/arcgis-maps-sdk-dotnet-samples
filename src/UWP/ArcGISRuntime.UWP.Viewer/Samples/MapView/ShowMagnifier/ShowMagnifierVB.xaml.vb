' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace ShowMagnifier

    Partial Public Class ShowMagnifierVB

        Public Sub New()

            InitializeComponent()

            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create New Map with basemap And initial location
            Dim myMap As Map = New Map(BasemapType.Topographic, 34.056295, -117.1958, 10)

            ' Enable magnifier
            MyMapView.InteractionOptions.IsMagnifierEnabled = True

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

    End Class

End Namespace
