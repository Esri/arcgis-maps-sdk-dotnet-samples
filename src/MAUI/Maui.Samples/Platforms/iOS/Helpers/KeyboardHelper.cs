using UIKit;

namespace ArcGIS.Platforms.Helpers
{
    public static partial class KeyboardHelper
    {
        public static void HideKeyboard()
        {
            UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .SelectMany(s => s.Windows)
                .FirstOrDefault(w => w.IsKeyWindow).EndEditing(true);
        }
    }
}
