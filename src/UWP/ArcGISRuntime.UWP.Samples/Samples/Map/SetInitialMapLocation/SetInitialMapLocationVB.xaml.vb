﻿' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace SetInitialMapLocation

    Partial Public Class SetInitialMapLocationVB

        Public Sub New()
            InitializeComponent()

            'initialize map with `imagery with labels` basemap and an initial location
            Dim myMap = New Map(BasemapType.ImageryWithLabels, -33.867886, -63.985, 15)
            'assign the map to the map view
            MyMapView.Map = myMap
        End Sub

    End Class
End Namespace