using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;

namespace ArcGISRuntime.UWP.Samples.AuthorEditSaveMap
{
        
    // Provides map data to an application
    // Note: in a ArcGIS Runtime for .NET template project, this class will be in a separate file: "MapViewModel.cs"
    public class MapViewModel : INotifyPropertyChanged
    {        
        // Store the map view used by the app
        private MapView _mapView;
        public MapView AppMapView
        {
            set { _mapView = value; }
        }

        // Dictionary associates basemap names with basemaps.
        private readonly Dictionary<string, Basemap> _basemapChoices = new Dictionary<string, Basemap>
        {
            {"Imagery", Basemap.CreateImagery()},
            {"Imagery with vector labels", Basemap.CreateImageryWithLabelsVector()},
            {"Navigation (vector)", Basemap.CreateNavigationVector()},
            {"Topographic", Basemap.CreateTopographic()},
            {"National Geographic", Basemap.CreateNationalGeographic()},
            {"Oceans", Basemap.CreateOceans()},
            {"OpenStreetMap", Basemap.CreateOpenStreetMap()}
        };

        // Read-only property to return the available basemap names
        public string[] BasemapChoices => _basemapChoices.Keys.ToArray();

        // Create a default map with the vector streets basemap
        private Map _map = new Map(Basemap.CreateStreets());
    
        // Gets or sets the map        
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
        }

        public void ChangeBasemap(string basemap)
        {
            // Apply the selected basemap choice
            _map.Basemap = _basemapChoices[basemap];
        }

        // Save the current map to ArcGIS Online. The initial extent, title, description, and tags are passed in.
        public async Task SaveNewMapAsync(Viewpoint initialViewpoint, string title, string description, string[] tags, RuntimeImage img)
        {
            // Get the ArcGIS Online portal 
            ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync(new Uri("https://www.arcgis.com/sharing/rest"));

            // Set the map's initial viewpoint using the extent (viewpoint) passed in
            _map.InitialViewpoint = initialViewpoint;

            // Save the current state of the map as a portal item in the user's default folder
            await _map.SaveAsAsync(agsOnline, null, title, description, tags, img, false);
        }

        public bool MapIsSaved
        {
            // Return True if the current map has a value for the Item property
            get { return (_map != null && _map.Item != null); }
        }

        public async void UpdateMapItem()
        {
            // Save the map
            await _map.SaveAsync();
            
            // Export the current map view for the item thumbnail
            RuntimeImage thumbnailImg = await _mapView.ExportImageAsync();

            // Get the file stream from the new thumbnail image
            Stream imageStream = await thumbnailImg.GetEncodedBufferAsync();

            // Update the item thumbnail
            (_map.Item as PortalItem).SetThumbnailWithImage(imageStream);
            await _map.SaveAsync();
        }

        // Raises the PropertyChanged event for a property
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
