using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace ArcGIS.ViewModels
{
    public partial class FlyoutMenuViewModel : ObservableObject
    {
        private readonly Dictionary<string, char> _categoryIcons = new Dictionary<string, char>()
        {
            { "Featured", (char)0xe0dc },
            { "Favorites", (char)0xe2a9 },
            { "Analysis", (char)0xe003 },
            { "Data", (char)0xe0b7 },
            { "Geometry", (char)0xe190 },
            { "Geoprocessing", (char)0xe180 },
            { "GraphicsOverlay", (char)0xe16a },
            { "Hydrography", (char)0xe0e1 },
            { "Layers", (char)0xe192 },
            { "Location", (char)0xe092 },
            { "Map", (char)0xe164 },
            { "MapView", (char)0xe011 },
            { "Network analysis", (char)0xe2ce },
            { "Scene", (char)0xe139 },
            { "SceneView", (char)0xe018 },
            { "Search", (char)0xe18e },
            { "Security", (char)0xe1b6 },
            { "Symbology", (char)0xe1de },
            { "Utility network", (char)0xe26b },
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

        public void CategorySelected(string categoryName)
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
