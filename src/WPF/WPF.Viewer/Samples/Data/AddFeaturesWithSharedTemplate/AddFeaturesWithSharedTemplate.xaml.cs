// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

#nullable enable

using Esri.ArcGISRuntime;
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
        description: "Create features from preset and group shared templates, then save or discard the local edits.",
        instructions: "Click on a shared template to create features. Draw geometry. Save or undo local edits.",
        tags: new[] { "edit", "feature", "preset", "shared template", "template" })]
    public partial class AddFeaturesWithSharedTemplate
    {
        public AddFeaturesWithSharedTemplate()
        {
            InitializeComponent();

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                MyMapView.GeometryEditor = new GeometryEditor();
                MyMapView.Map = new Map(new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=dd64a70d17de4f16a93d2203c4cf1ab3"));

                Map map = MyMapView.Map ?? throw new InvalidOperationException("The map view does not have a map.");
                await map.LoadAsync();

                ServiceGeodatabase? serviceGeodatabase = null;

                foreach (var layer in map.OperationalLayers)
                {
                    if (serviceGeodatabase is not null)
                    {
                        break;
                    }

                    if (layer is GroupLayer groupLayer)
                    {
                        foreach (var childLayer in groupLayer.Layers)
                        {
                            if (childLayer is FeatureLayer featureLayer && featureLayer.FeatureTable is ServiceFeatureTable sft)
                            {
                                serviceGeodatabase = sft.ServiceGeodatabase;
                                break;
                            }
                        }
                    }
                }

                if (serviceGeodatabase is null)
                {
                    throw new InvalidOperationException("The map does not contain a service geodatabase.");
                }

                IReadOnlyDictionary<long, IReadOnlyList<SharedTemplate>> templatesByLayer = await serviceGeodatabase.QuerySharedTemplatesAsync();
                var templateItems = new List<TemplatePickerItem>();

                foreach (KeyValuePair<long, IReadOnlyList<SharedTemplate>> templatesForLayer in templatesByLayer)
                {
                    if (templateItems.Count == 2)
                    {
                        break;
                    }

                    foreach (SharedTemplate template in templatesForLayer.Value)
                    {
                        if (template.Type != SharedTemplateType.Preset && template.Type != SharedTemplateType.Group)
                        {
                            continue;
                        }

                        if (templateItems.Any(item => item.Template.Type == template.Type))
                        {
                            continue;
                        }

                        GeometryConstructionTool? constructionTool = template.GetDefaultConstructionTool(templatesForLayer.Key);
                        if (constructionTool?.ToolType != GeometryConstructionToolType.Point
                            && constructionTool?.ToolType != GeometryConstructionToolType.Line)
                        {
                            continue;
                        }

                        ImageSource imageSource = new CalciteIconImageExtension
                        {
                            Icon = CalciteIcon.AddFeatures,
                            SymbolSize = 36
                        }.ProvideValue(null!) as ImageSource
                            ?? throw new InvalidOperationException("Unable to create the fallback template image.");

                        try
                        {
                            RuntimeImage swatch = await template.CreateSwatchAsync(templatesForLayer.Key);
                            imageSource = await swatch.ToImageSourceAsync() ?? imageSource;
                        }
                        catch (Exception)
                        {
                        }

                        templateItems.Add(new TemplatePickerItem(template, templatesForLayer.Key, imageSource));

                        if (templateItems.Count == 2)
                        {
                            break;
                        }
                    }
                }

                TemplatePicker.ItemsSource = templateItems;

                StatusTextBlock.Text = templateItems.Count > 0
                    ? "Click on a shared template to create features."
                    : "No supported shared templates are available.";
            }
            catch (Exception ex)
            {
                ShowError(ex);
                StatusTextBlock.Text = "Unable to load the shared templates.";
            }
        }

        private void OnSharedTemplateClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: TemplatePickerItem templateItem }
                || MyMapView.GeometryEditor?.IsStarted == true)
            {
                return;
            }

            try
            {
                GeometryConstructionTool constructionTool = templateItem.Template.GetDefaultConstructionTool(templateItem.LayerId)
                    ?? throw new InvalidOperationException("The shared template does not provide a default geometry construction tool.");
                GeometryType geometryType = constructionTool.ToolType switch
                {
                    GeometryConstructionToolType.Point => GeometryType.Point,
                    GeometryConstructionToolType.Line => GeometryType.Polyline,
                    _ => throw new NotSupportedException($"The {constructionTool.ToolType} geometry construction tool is not supported by this sample.")
                };
                TemplatePicker.Tag = templateItem;
                StatusTextBlock.Text = "Place a point or sketch a polyline, then click Complete.";

                GeometryEditor geometryEditor = MyMapView.GeometryEditor
                    ?? throw new InvalidOperationException("The map view does not have a geometry editor.");
                geometryEditor.Tool = new VertexTool();
                geometryEditor.Start(geometryType);
            }
            catch (Exception ex)
            {
                ShowError(ex);
                ClearActiveTemplate();
                StatusTextBlock.Text = "Unable to start drawing. Click on a shared template to create features again.";
            }
        }

        private async void OnDrawCompleted(object sender, RoutedEventArgs e)
        {
            if (TemplatePicker.Tag is not TemplatePickerItem templateItem
                || MyMapView.GeometryEditor is not { IsStarted: true } geometryEditor)
            {
                return;
            }

            Geometry? geometry = geometryEditor.Stop();

            if (geometry is null || geometry.IsEmpty)
            {
                ClearActiveTemplate();
                StatusTextBlock.Text = "No geometry was drawn. Click on a shared template to create features again.";
                return;
            }

            try
            {
                StatusTextBlock.Text = $"Creating {templateItem.Template.Name}.";
                if (templateItem.Template.TemplateSource is not ServiceGeodatabase serviceGeodatabase)
                {
                    throw new InvalidOperationException("The shared template is not backed by a service geodatabase.");
                }

                SharedTemplateFeatureCreationSet featureCreationSet = await serviceGeodatabase.CreateFeaturesAsync(templateItem.Template, geometry);
                await serviceGeodatabase.AddFeaturesAsync(featureCreationSet);

                PendingEditsPanel.Visibility = Visibility.Visible;
                StatusTextBlock.Text = "Save or cancel the local edit.";
            }
            catch (Exception ex)
            {
                ShowError(ex);
                ClearActiveTemplate();
                StatusTextBlock.Text = "Unable to create features. Click on a shared template to create features again.";
            }
        }

        private void OnDrawCanceled(object sender, RoutedEventArgs e)
        {
            MyMapView.GeometryEditor?.Stop();
            ClearActiveTemplate();
            StatusTextBlock.Text = "Click on a shared template to create features.";
        }

        private async void OnEditsSaved(object sender, RoutedEventArgs e)
        {
            if (TemplatePicker.Tag is not TemplatePickerItem { Template.TemplateSource: ServiceGeodatabase serviceGeodatabase })
            {
                return;
            }

            try
            {
                StatusTextBlock.Text = "Saving edits.";
                await serviceGeodatabase.ApplyEditsAsync();
                CompletePendingEdits("Edits saved. Click on a shared template to create features again.");
            }
            catch (Exception ex)
            {
                ShowError(ex);
                StatusTextBlock.Text = "Unable to save edits. Save again or cancel them.";
            }
        }

        private async void OnEditsUndone(object sender, RoutedEventArgs e)
        {
            if (TemplatePicker.Tag is not TemplatePickerItem { Template.TemplateSource: ServiceGeodatabase serviceGeodatabase })
            {
                return;
            }

            try
            {
                StatusTextBlock.Text = "Canceling local edits.";
                await serviceGeodatabase.UndoLocalEditsAsync();
                CompletePendingEdits("Edits undone. Click on a shared template to create features again.");
            }
            catch (Exception ex)
            {
                ShowError(ex);
                StatusTextBlock.Text = "Unable to cancel local edits.";
            }
        }

        private void CompletePendingEdits(string status)
        {
            PendingEditsPanel.Visibility = Visibility.Collapsed;
            ClearActiveTemplate();
            StatusTextBlock.Text = status;
        }

        private void ClearActiveTemplate()
        {
            TemplatePicker.Tag = null;
        }

        private static void ShowError(Exception exception)
        {
            MessageBox.Show(exception.Message, exception.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
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