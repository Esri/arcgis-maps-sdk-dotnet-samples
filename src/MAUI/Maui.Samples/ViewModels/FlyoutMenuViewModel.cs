using CommunityToolkit.Mvvm.ComponentModel;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ArcGIS.Helpers;

namespace ArcGIS.ViewModels
{
    public partial class FlyoutMenuViewModel : ObservableObject
    {
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
                categories.Add(new FlyoutCategoryViewModel(category.Name));
            }

            Categories = new ObservableCollection<FlyoutCategoryViewModel>(categories);
            
        }

        [ObservableProperty]
        private ObservableCollection<FlyoutCategoryViewModel> _categories;
    }

    public partial class FlyoutCategoryViewModel : ObservableObject
    {
        public FlyoutCategoryViewModel(string categoryName)
        {
            CategoryName = categoryName;
            Route = $"{nameof(CategoryPage)}_{categoryName}";
        }

        [ObservableProperty]
        private string _categoryName;

        [ObservableProperty]
        private string _route;
    }
}
