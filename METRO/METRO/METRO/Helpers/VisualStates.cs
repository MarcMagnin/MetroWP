using System;
using System.Windows;
using System.Windows.Controls;

namespace METRO.Helpers
{
    public static class VisualStates
    {
     
         public class VisualStateEventArgs : EventArgs
        {
             public VisualStateEventArgs(DependencyPropertyChangedEventArgs e)
            {
                EventArgs = e;
            }

            public DependencyPropertyChangedEventArgs EventArgs { get; private set; }
        }


         public static event EventHandler<VisualStateEventArgs> VisualStateChanged;


        static readonly DependencyProperty CurrentVisualStateProperty = DependencyProperty.RegisterAttached("CurrentVisualState", typeof(String), typeof(VisualStates), new PropertyMetadata(TransitionToMasterState));
        public static string GetCurrentVisualState(DependencyObject obj)
        {
            return (string)obj.GetValue(CurrentVisualStateProperty);
        }
        public static void SetCurrentVisualState(DependencyObject obj, string value)
        {
            obj.SetValue(CurrentVisualStateProperty, value);
        }
        private static void TransitionToMasterState(object sender, DependencyPropertyChangedEventArgs args)
        {
            if(previousState !=(string)args.NewValue )
            {
                if(VisualStateChanged != null)
                    VisualStateChanged(args.NewValue, new VisualStateEventArgs(args));
                previousState = (string) args.NewValue;
            }
            var c = sender as Control; if (c != null)
            {
                VisualStateManager.GoToState(c, (string)args.NewValue, true);
            }
            else { throw new ArgumentException("CurrentState is only supported on the Control type"); }
        }

        private static string previousState;
    }
}
