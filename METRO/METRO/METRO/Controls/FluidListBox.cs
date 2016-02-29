using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace METRO.Controls
{
    public class FluidListBox : ListBox
    {
        public FluidListBox()
        {
            //this.ItemContainerGenerator.ItemsChanged += new System.Windows.Controls.Primitives.ItemsChangedEventHandler(ItemContainerGenerator_ItemsChanged);
        }

        void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            foreach (var item in Items)
            {
                var listBoxItem = ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if(listBoxItem == null)
                    return;
                listBoxItem.Visibility = Visibility.Visible;
                VisualStateManager.GoToState(listBoxItem, "Show", true);
            }
        }
    }
}
