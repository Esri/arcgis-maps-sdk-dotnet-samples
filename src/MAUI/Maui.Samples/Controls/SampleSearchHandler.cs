using ArcGIS.Helpers;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;

namespace ArcGIS.Controls
{
    public class SampleSearchHandler : SearchHandler
    {
        public IList<SampleInfo> Samples { get; set; }
        public Type SelectedItemNavigationTarget { get; set; }

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                var allSamples = SampleManager.Current.AllSamples.ToList();
                string searchText = newValue.ToLower();

                ItemsSource = allSamples.Where(sample => sample.SampleName.ToLower().Contains(searchText) ||
                   sample.Category.ToLower().Contains(searchText) ||
                   sample.Description.ToLower().Contains(searchText) ||
                   sample.Tags.Any(tag => tag.Contains(searchText))).ToList<SampleInfo>();
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);

            // Let animation complete.
            await Task.Delay(1000);

            await SampleLoader.LoadSample((SampleInfo)item, Shell.Current.CurrentPage);

#if iOS || ANDROID
            ArcGIS.Platforms.Helpers.KeyboardHelper.HideKeyboard();
#endif
        }
    }
}
