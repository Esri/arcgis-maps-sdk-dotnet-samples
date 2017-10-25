' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping

Namespace ServiceFeatureTableNoCache

    Public Class ServiceFeatureTableNoCacheVB

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create and set initial map area
            Dim initialLocation As New Envelope(-13075816.4047166, 4014771.46954516, -13073005.6797177, 4016869.78617381, SpatialReferences.WebMercator)
            myMap.InitialViewpoint = New Viewpoint(initialLocation)

            ' Create uri to the used feature service
            Dim serviceUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0")

            ' Create feature table for the pools feature service
            Dim poolsFeatureTable As New ServiceFeatureTable(serviceUri)

            ' Define the request mode
            poolsFeatureTable.FeatureRequestMode = FeatureRequestMode.OnInteractionNoCache

            ' Create FeatureLayer that uses the created table
            Dim poolsFeatureLayer As New FeatureLayer(poolsFeatureTable)

            ' Add created layer to the map
            myMap.OperationalLayers.Add(poolsFeatureLayer)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

    End Class

End Namespace
