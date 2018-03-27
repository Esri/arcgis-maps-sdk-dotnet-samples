using System;

using Xamarin.Forms;

namespace ArcGISRuntime.Samples.AuthorEditSaveMap
{
	public partial class SaveMapPage : ContentPage
	{
        // Raise an event so the listener can access input values when the form has been completed
        public event EventHandler<SaveMapEventArgs> OnSaveClicked;

        public SaveMapPage ()
		{
			InitializeComponent ();
		}

        // A click handler for the save map button
        private void SaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Get information for the new portal item
                var title = MapTitleEntry.Text;
                var description = MapDescriptionEntry.Text;
                var tags = MapTagsEntry.Text.Split(',');

                // Make sure all required info was entered
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || tags.Length == 0)
                {
                    throw new Exception("Please enter a title, description, and some tags to describe the map.");
                }

                // Create a new SaveMapEventArgs object to store the information entered by the user
                var mapSavedArgs = new SaveMapEventArgs(title, description, tags);

                // Raise the OnSaveClicked event so the main page can handle the event and save the map
                OnSaveClicked(this, mapSavedArgs);

                // Close the dialog
                Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                // Show the exception message (dialog will stay open so user can try again)
                DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            // If the user cancels, just navigate back to the previous page
            Navigation.PopAsync();
        }
    }

    // Custom EventArgs class for containing portal item properties when saving a map
    public class SaveMapEventArgs : EventArgs
    {
        // Portal item title
        public string MapTitle { get; set; }

        // Portal item description
        public string MapDescription { get; set; }

        // Portal item tags
        public string[] Tags { get; set; }

        public SaveMapEventArgs(string title, string description, string[] tags) : base()
        {
            MapTitle = title;
            MapDescription = description;
            Tags = tags;
        }
    }
}
