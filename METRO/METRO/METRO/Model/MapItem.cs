using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Expression.Interactivity.Core;

namespace METRO.Model
{
    public class MapItem : INotifyPropertyChanged
    {
        public int Id{ get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public SolidColorBrush Color { get; set; }
        public Control ItemControl { get; set; }

        public bool Selected { get; set; }
        public virtual void Select()
        {
            Selected = true;
           // ExtendedVisualStateManager.GoToElementState(ItemControl, "Selected", true);
        }

        public void UnSelect()
        {
            Selected = false;
           // ExtendedVisualStateManager.GoToElementState(ItemControl, "Normal", true);
        }


        public static DependencyProperty ItemControlSetterProperty = DependencyProperty.RegisterAttached("ItemControlSetter",
                                                                                                 typeof(MapItem
                                                                                                     ),
                                                                                                 typeof(FrameworkElement
                                                                                                     ),
                                                                                                 new PropertyMetadata
                                                                                                     (OnItemControlSetterChanged));

        public static FrameworkElement GetItemControlSetter(DependencyObject obj)
        {
            return (FrameworkElement)obj.GetValue(ItemControlSetterProperty);
        }

        public static void SetItemControlSetter(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ItemControlSetterProperty, value);
        }
        public event EventHandler ItemControlChanged;
        public static void OnItemControlSetterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            if (element == null)
                return;

            var item = element.DataContext as MapItem;
            if (item == null)
                return;

            item.ItemControl = element as Control;

            // Signal au model qu'il peut exploiter l'itemControl pour y attacher les events
            if (item.ItemControlChanged != null)
                item.ItemControlChanged(null, null);
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion Implementation of INotifyPropertyChanged
    }
}
