#if WINDOWS || MACCATALYST
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Controls
{
    public class ExpandButtonContent : ContentView
    {
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var node = BindingContext as TreeViewNode;
            bool isLeafNode = (node.ChildrenList == null || node.ChildrenList.Count == 0);

            if (!isLeafNode)
            {
                AppTheme currentTheme = Application.Current.RequestedTheme;

                string minusResource = currentTheme == AppTheme.Light ? "minus.png" : "minusdark.png";
                string plusResource = currentTheme == AppTheme.Light ? "plus.png" : "plusdark.png";

                Content = new ResourceImage
                {
                    Resource = node.IsExpanded ? minusResource : plusResource,
                    HeightRequest = 16,
                    WidthRequest = 16
                };
            }
        }
    }
}
#endif