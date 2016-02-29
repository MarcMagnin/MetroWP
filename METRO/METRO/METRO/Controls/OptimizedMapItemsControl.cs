
using System;
using System.Windows;
using System.Windows.Controls;
using METRO.Helpers;
using METRO.Model;
using METRO.ViewModel;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Reactive;

namespace METRO.Controls
{
    public class OptimizedMapItemsControl : MapItemsControl
    {
        #region Private Properties

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public int MinZoomVisibility
        {
            get { return (int)GetValue(MinZoomVisibilityProperty); }
            set { SetValue(MinZoomVisibilityProperty, value); }
        }

        public static readonly DependencyProperty MinZoomVisibilityProperty =
        DependencyProperty.Register("MinZoomVisibility",typeof(int), typeof(OptimizedMapItemsControl), new PropertyMetadata(14, OnMinZoomVisibilityChanged));

        private static void OnMinZoomVisibilityChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((OptimizedMapItemsControl)o).OnMinZoomVisibilityChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual void OnMinZoomVisibilityChanged(int oldValue, int newValue)
        {
            //
        }

        private Visibility _visibility;

        #endregion

        //protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        //{
        //    base.PrepareContainerForItemOverride(element, item);
        //    if(item is Station)
        //        (element as ContentPresenter).ContentTemplate = Application.Current.Resources["MetroStationTemplate"] as DataTemplate;
        //    else
        //        (element as ContentPresenter).ContentTemplate = Application.Current.Resources["MetroLineTemplate"] as DataTemplate;

        //}

        #region Constructor

        ///<summary>
        /// Constructor
        ///</summary>
        public OptimizedMapItemsControl()
        {
            Loaded += (sender, evt) =>
            {
                var map = (Map)ParentMap;
                IDisposable o = null;
                map.ViewChangeStart +=(s, e) =>
                {
                    
                    Visibility = Visibility.Collapsed;
                    if (o != null)
                        o.Dispose();
                    o = Observable.FromEvent<MapEventArgs>(map, "ViewChangeEnd")
                    .Throttle(TimeSpan.FromMilliseconds(3000))
                    .ObserveOn(Deployment.Current.Dispatcher)
                    .Subscribe(unused =>
                    {
                        Visibility = Visibility.Visible;
                    });
                };
            };
        }

        #endregion

        #region Public Properties

     

        #endregion
    }
}
 

