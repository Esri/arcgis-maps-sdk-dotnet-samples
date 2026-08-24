// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

#nullable enable

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Editing;
using Esri.Calcite.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ImageSource = System.Windows.Media.ImageSource;

namespace ArcGIS.WPF.Samples.AddFeaturesWithSharedTemplate
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Add features with shared templates",
        category: "Data",
        description: "Create features from preset and group shared templates.",
        instructions: "Hover over a shared template to view its description. Select a template and click on the map to place the geometry. Choose \"Complete\" to create the feature, \"Save\" to apply local edits, or \"Undo\" to discard them.",
        tags: new[] { "edit", "feature", "group", "preset", "shared template", "shared template source", "template" })]
    public partial class AddFeaturesWithSharedTemplate
    {
        private const string Instruction = "Click on a shared template to create features.";
        private TemplatePickerItem? _activeTemplateItem = null;

        public AddFeaturesWithSharedTemplate()
        {
            InitializeComponent();

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                MyMapView.GeometryEditor ??= new GeometryEditor();          

                // Load a web map that contains a feature service with shared templates.
                var map = new Map(new Uri("https://www.maps.arcgis.com/home/item.html?id=b635be46dfb545b888077389ac7f0962"));
                MyMapView.Map = map;
                await map.LoadAsync();

                // Find the shared template source in a service-backed feature layer.
                ISharedTemplateSource sharedTemplateSource = 
                    map.OperationalLayers
                       .OfType<FeatureLayer>()
                       .Select(layer => (layer.FeatureTable as ServiceFeatureTable)?.ServiceGeodatabase)
                       .OfType<ISharedTemplateSource>()
                       .FirstOrDefault() ?? throw new InvalidOperationException("The map does not contain a shared template source.");

                // Query without parameters will return all shared templates for all the layers in the map
                IReadOnlyDictionary<long, IReadOnlyList<SharedTemplate>> templatesByLayer = await sharedTemplateSource.QuerySharedTemplatesAsync();
                var templateItems = new List<TemplatePickerItem>();

                foreach (KeyValuePair<long, IReadOnlyList<SharedTemplate>> templatesForLayer in templatesByLayer)
                {
                    if (templateItems.Count == 2)
                    {
                        break;
                    }

                    foreach (SharedTemplate template in templatesForLayer.Value)
                    {
                        if (templateItems.Count == 2)
                        {
                            break;
                        }

                        if ((template.Type != SharedTemplateType.Preset && template.Type != SharedTemplateType.Group)
                            || templateItems.Any(item => item.Template.Type == template.Type))
                        {
                            continue;
                        }

                        // Create a default swatch image for the template
                        ImageSource imageSource = new CalciteIconImageExtension
                        {
                            Icon = CalciteIcon.AddFeatures,
                            SymbolSize = 36
                        }.ProvideValue(null!) as ImageSource
                            ?? throw new InvalidOperationException("Unable to create a default swatch image.");

                        try
                        {
                            // Generate a swatch image for this template
                            RuntimeImage swatch = await template.CreateSwatchAsync(templatesForLayer.Key);
                            imageSource = await swatch.ToImageSourceAsync() ?? imageSource;
                        }
                        catch (Exception)
                        {
                            // Template does not provide a swatch
                        }

                        templateItems.Add(new TemplatePickerItem(template, templatesForLayer.Key, imageSource));
                    }
                }

                TemplatePicker.ItemsSource = templateItems;
                TemplatePicker.Visibility = Visibility.Visible;
                StatusTextBlock.Text = Instruction;
            }
            catch (Exception ex)
            {
                UpdateUI("Unable to load templates.", ex);
            }
        }

        private void UpdateUI(string status, Exception? exception = null)
        {
            if (exception is not null)
            {
                MessageBox.Show(exception.Message, exception.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_activeTemplateItem?.Template.TemplateSource is ServiceGeodatabase serviceGeodatabase)
            {
                PendingEditsPanel.Visibility = serviceGeodatabase.HasLocalEdits() ? Visibility.Visible : Visibility.Collapsed;
                TemplatePicker.Visibility = serviceGeodatabase.HasLocalEdits() ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                TemplatePicker.Visibility = Visibility.Visible;
                PendingEditsPanel.Visibility = Visibility.Collapsed;
            }
            MyMapView.GeometryEditor?.Stop();
            DrawingButtonsPanel.Visibility = Visibility.Collapsed;
            StatusTextBlock.Text = $"{status} {Instruction}";
        }

        private void OnSharedTemplateClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button 
                || button.Tag is not TemplatePickerItem templateItem
                || MyMapView.GeometryEditor is not GeometryEditor geometryEditor
                || geometryEditor.IsStarted)
            {
                return;
            }

            try
            {
                TemplatePicker.Visibility = Visibility.Collapsed;
                DrawingButtonsPanel.Visibility = Visibility.Visible;
                _activeTemplateItem = templateItem;

                GeometryConstructionToolType constructionToolType = _activeTemplateItem.Template.GetDefaultConstructionTool(_activeTemplateItem.LayerId)?.ToolType 
                    ?? GeometryConstructionToolType.Unknown;

                // Use the construction tool type to choose whether to draw a point or a polyline
                GeometryType geometryType = constructionToolType switch
                {
                    GeometryConstructionToolType.Point => GeometryType.Point,
                    GeometryConstructionToolType.Line => GeometryType.Polyline,
                    _ => throw new NotSupportedException($"The {constructionToolType} geometry construction tool is not supported by this sample.")
                };

                StatusTextBlock.Text = geometryType == GeometryType.Point
                    ? "Place a point, then click Complete or Cancel."
                    : "Sketch a line, then click Complete or Cancel.";

                geometryEditor.Tool = new VertexTool();
                geometryEditor.Start(geometryType);
            }
            catch (Exception ex)
            {
                UpdateUI("Unable to start drawing.", ex);
            }
        }

        private async void OnGeometryEditorCompleted(object sender, RoutedEventArgs e)
        {
            if (_activeTemplateItem is null 
                || _activeTemplateItem.Template.TemplateSource is not ISharedTemplateSource sharedTemplateSource
                || MyMapView.GeometryEditor is not GeometryEditor geometryEditor
                || !geometryEditor.IsStarted)
            {
                return;
            }

            Geometry? geometry = geometryEditor.Stop();
            DrawingButtonsPanel.Visibility = Visibility.Collapsed;

            if (geometry is null || geometry.IsEmpty)
            {
                UpdateUI("No geometry was drawn.");
                return;
            }

            try
            {

                // Creates in-memory features from different layers with default geometry and attributes.
                SharedTemplateFeatureCreationSet featureCreationSet = await sharedTemplateSource.CreateFeaturesAsync(_activeTemplateItem.Template, geometry);

                // Note: You can continue to make attribute changes to this feature creation set.

                // Commits the feature creation set to the database.
                await sharedTemplateSource.AddFeaturesAsync(featureCreationSet);

                PendingEditsPanel.Visibility = Visibility.Visible;
                StatusTextBlock.Text = "Save or undo edits.";
            }
            catch (Exception ex)
            {
                UpdateUI("Unable to create or add features.", ex);
            }
        }

        private void OnGeometryEditorCanceled(object sender, RoutedEventArgs e)
        {
            UpdateUI("Draw canceled.");
        }

        private async void OnEditsSaved(object sender, RoutedEventArgs e)
        {
            if (_activeTemplateItem?.Template.TemplateSource is not ServiceGeodatabase serviceGeodatabase)
            {
                return;
            }

            try
            {
                StatusTextBlock.Text = "Saving edits.";
                await serviceGeodatabase.ApplyEditsAsync();
                UpdateUI("Edits saved.");
            }
            catch (Exception ex)
            {
                UpdateUI("Unable to save edits.", ex);
            }
        }

        private async void OnEditsUndone(object sender, RoutedEventArgs e)
        {
            if (_activeTemplateItem?.Template.TemplateSource is not ServiceGeodatabase serviceGeodatabase)
            {
                return;
            }

            try
            {
                StatusTextBlock.Text = "Undoing local edits.";
                await serviceGeodatabase.UndoLocalEditsAsync();
                UpdateUI("Edits undone.");  
            }
            catch (Exception ex)
            {
                UpdateUI("Unable to undo edits.", ex);
            }
        }
    }

    internal sealed class TemplatePickerItem
    {
        internal TemplatePickerItem(SharedTemplate template, long layerId, ImageSource imageSource)
        {
            Template = template;
            LayerId = layerId;
            ImageSource = imageSource;
        }

        public SharedTemplate Template { get; }

        public long LayerId { get; }

        public ImageSource ImageSource { get; }
    }
}