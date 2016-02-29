using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace METRO.Helpers
{
    public static class TreeHelper
    {
        public static IEnumerable<T> GetControlsDecendant<T>(object reference) where T : DependencyObject
        {
            return GetControlsDecendant<T>(reference as DependencyObject);
        }

        public static IEnumerable<T> GetControlsDecendant<T>(DependencyObject reference) where T : DependencyObject
        {
            if (reference == null)
                yield break;
            var nbChildren = VisualTreeHelper.GetChildrenCount(reference);
            for (var index = 0; index < nbChildren; index++)
            {
                var child = VisualTreeHelper.GetChild(reference, index);
                var value = child as T;
                if (value != null)
                    yield return value;
                foreach (var childValue in GetControlsDecendant<T>(child))
                    yield return childValue;
            }
        }
    }
}
