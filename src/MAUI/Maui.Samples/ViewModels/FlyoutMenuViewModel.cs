using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Esri.Calcite.Maui;

namespace ArcGIS.ViewModels
{
    public partial class FlyoutMenuViewModel : ObservableObject
    {
        private readonly Dictionary<string, char> _categoryIcons = new Dictionary<string, char>()
        {
            { "Featured", (char)CalciteIcon.Effects },
            { "Favorites", (char)CalciteIcon.Star },
            { "Analysis", (char)CalciteIcon._3DGlasses },
            { "Data", (char)CalciteIcon.DataFolder },
            { "Geometry", (char)CalciteIcon.LayersEditable },
            { "Geoprocessing", (char)CalciteIcon.LayerBasemap },
            { "GraphicsOverlay", (char)CalciteIcon.Images },
            { "Hydrography", (char)CalciteIcon.WaterDrop },
            { "Layers", (char)CalciteIcon.Layers },
            { "Location", (char)CalciteIcon.CompassNeedle },
            { "Map", (char)CalciteIcon.ImagePin },
            { "MapView", (char)CalciteIcon.Analysis },
            { "Network analysis", (char)CalciteIcon.Tour },
            { "Scene", (char)CalciteIcon.Globe },
            { "SceneView", (char)CalciteIcon.Applications },
            { "Search", (char)CalciteIcon.LayerZoomTo },
            { "Security", (char)CalciteIcon.Lock },
            { "Symbology", (char)CalciteIcon.MultipleVariables },
            { "Utility network", (char)CalciteIcon.SelectRange },
        };

        public FlyoutMenuViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            SampleManager.Current.Initialize();

            var samplesCategories = SampleManager.Current.FullTree.Items.OfType<SearchableTreeNode>().ToList();

            var categories = new List<FlyoutCategoryViewModel>();

            foreach (var category in samplesCategories)
            {
                if (_categoryIcons.TryGetValue(category.Name, out char categoryIcon))
                {
                    var flyoutCategory = new FlyoutCategoryViewModel(category.Name, categoryIcon);
                    flyoutCategory.IsSelected = category.Name == "Featured";
                    categories.Add(flyoutCategory);
                }
                else
                {
                    var flyoutCategory = new FlyoutCategoryViewModel(category.Name, (char)0xe1e2);
                    flyoutCategory.IsSelected = category.Name == "Featured";
                    categories.Add(flyoutCategory);
                }
            }

            Categories = new ObservableCollection<FlyoutCategoryViewModel>(categories);

        }

        [ObservableProperty]
        private ObservableCollection<FlyoutCategoryViewModel> _categories;

        public static void CategorySelected(string categoryName)
        {
            WeakReferenceMessenger.Default.Send(categoryName);
        }
    }

    public partial class FlyoutCategoryViewModel : ObservableObject
    {
        public FlyoutCategoryViewModel(string categoryName, char categoryIcon)
        {
            CategoryName = categoryName;
            CategoryIcon = categoryIcon;
        }

        [ObservableProperty]
        private string _categoryName;

        [ObservableProperty]
        private char _categoryIcon;

        [ObservableProperty]
        private bool _isSelected;
    }
}
