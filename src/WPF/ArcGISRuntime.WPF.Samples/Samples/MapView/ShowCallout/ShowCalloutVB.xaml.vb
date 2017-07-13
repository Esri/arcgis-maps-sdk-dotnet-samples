' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.UI.Controls
Imports System.Windows
Imports System

Namespace ShowCallout

    Partial Public Class ShowCalloutVB

        Public Sub New()

            InitializeComponent()

            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a new Map instance with the basemap  
            Dim myBasemap As Basemap = Basemap.CreateImageryWithLabels()
            Dim myMap As New Map(myBasemap)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Sub MyMapView_GeoViewTapped(sender As Object, e As GeoViewInputEventArgs) Handles MyMapView.GeoViewTapped
            ' Get tap location
            Dim mapLocation As MapPoint = e.Location

            '' Convert to Traditional Lat/Lng display
            Dim projectedLocation As MapPoint = (MapPoint)GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84)

            ' Format string for display
            Dim mapLocationString As String = String.Format("Lat: {0:F3} Lng:{1:F3}", projectedLocation.Y, projectedLocation.X)

            ' Display Callout
            MyMapView.ShowCalloutAt(mapLocation, new Esri.ArcGISRuntime.UI.CalloutDefinition("Location:", mapLocationString))
        End Sub

    End Class

End Namespace
