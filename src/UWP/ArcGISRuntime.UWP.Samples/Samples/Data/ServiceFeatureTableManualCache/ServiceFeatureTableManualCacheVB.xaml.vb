' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping

Namespace ServiceFeatureTableManualCache

    Public Class ServiceFeatureTableManualCacheVB

        Private _incidentsFeatureTable As ServiceFeatureTable

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create and set initial map location
            Dim initialLocation As New MapPoint(-13630484, 4545415, SpatialReferences.WebMercator)
            myMap.InitialViewpoint = New Viewpoint(initialLocation, 500000)

            ' Create uri to the used feature service
            Dim serviceUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0")

            ' Create feature table for the incident feature service
            _incidentsFeatureTable = New ServiceFeatureTable(serviceUri)

            ' Define the request mode
            _incidentsFeatureTable.FeatureRequestMode = FeatureRequestMode.ManualCache

            ' When feature table is loaded, populate data
            AddHandler _incidentsFeatureTable.LoadStatusChanged, AddressOf OnLoadedPopulateData

            ' Create FeatureLayer that uses the created table
            Dim incidentsFeatureLayer As New FeatureLayer(_incidentsFeatureTable)

            ' Add created layer to the map
            myMap.OperationalLayers.Add(incidentsFeatureLayer)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Async Sub OnLoadedPopulateData(ByVal sender As Object, ByVal e As LoadStatusEventArgs)

            ' If layer isn't loaded, do nothing
            If e.Status <> LoadStatus.Loaded Then
                Return
            End If

            ' Create new query object that contains parameters to query specific request types
            Dim queryParameters As New QueryParameters() With
                {
                .WhereClause = "req_Type = 'Tree Maintenance or Damage'"
                }

            ' Create list of the fields that are returned from the service
            Dim outputFields = New String() {"*"}

            ' Populate feature table with the data based on query
            Await _incidentsFeatureTable.PopulateFromServiceAsync(queryParameters, True, outputFields)

        End Sub

    End Class

End Namespace
