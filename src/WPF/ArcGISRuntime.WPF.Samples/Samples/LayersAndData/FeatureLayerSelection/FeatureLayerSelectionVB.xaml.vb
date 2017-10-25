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
Imports Esri.ArcGISRuntime.UI.Controls
Imports System.Windows
Imports System.Windows.Media

Namespace FeatureLayerSelection
    Partial Public Class FeatureLayerSelectionVB

        ' Create and hold reference to the feature layer
        Private _featureLayer As FeatureLayer

        Public Sub New()
            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Async Sub Initialize()
            ' Create new Map with basemap
            Dim myMap = New Map(Basemap.CreateTopographic())

            ' Create envelope to be used as a target extent for map's initial viewpoint
            Dim myEnvelope As New Envelope(-1131596.019761, 3893114.069099, 3926705.98214, 7977912.46179, SpatialReferences.WebMercator)

            ' Set the initial viewpoint for map
            myMap.InitialViewpoint = New Viewpoint(myEnvelope)

            ' Provide used Map to the MapView
            MyMapView.Map = myMap

            ' Create Uri for the feature service
            Dim featureServiceUri As New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0")

            ' Initialize feature table using a url to feature server url
            Dim featureTable = New ServiceFeatureTable(featureServiceUri)

            ' Initialize a new feature layer based on the feature table
            _featureLayer = New FeatureLayer(featureTable)

            ' Set the selection color for feature layer
            _featureLayer.SelectionColor = Colors.Cyan

            ' Set the selection width
            _featureLayer.SelectionWidth = 3

            ' Make sure that used feature layer is loaded before we hook into the tapped event
            ' This prevents us trying to do selection on the layer that isn't initialized
            Await _featureLayer.LoadAsync()

            ' Check for the load status. If the layer is loaded then add it to map
            If _featureLayer.LoadStatus = LoadStatus.Loaded Then
                ' Add the feature layer to the map
                myMap.OperationalLayers.Add(_featureLayer)

                ' Add tap event handler for map view
                AddHandler MyMapView.GeoViewTapped, AddressOf OnMapViewTapped
            End If
        End Sub

        Private Async Sub OnMapViewTapped(sender As Object, e As GeoViewInputEventArgs)
            Try
                ' Define the selection tolerance (half the marker symbol size so that any click on the symbol will select the feature)
                Dim tolerance As Double = 14

                ' Convert the tolerance to map units
                Dim mapTolerance As Double = tolerance * MyMapView.UnitsPerPixel

                ' Define the envelope around the tap location for selecting features
                Dim selectionEnvelope = New Envelope(e.Location.X - mapTolerance, e.Location.Y - mapTolerance, e.Location.X + mapTolerance, e.Location.Y + mapTolerance, MyMapView.Map.SpatialReference)

                ' Define the query parameters for selecting features
                Dim queryParams = New QueryParameters()

                ' Set the geometry to selection envelope for selection by geometry
                queryParams.Geometry = selectionEnvelope

                ' Select the features based on query parameters defined above
                Await _featureLayer.SelectFeaturesAsync(queryParams, Esri.ArcGISRuntime.Mapping.SelectionMode.[New])
            Catch ex As Exception
                MessageBox.Show("Sample error", ex.ToString())
            End Try
        End Sub
    End Class
End Namespace

