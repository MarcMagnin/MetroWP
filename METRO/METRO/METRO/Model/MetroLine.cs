using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls.Maps;

namespace METRO.Model
{
    public class MetroLine :MapItem
    {
        public MetroLine()
        {
            ItemControlChanged += ThisItemControlChanged;
        }

        void ThisItemControlChanged(object sender, System.EventArgs e)
        {
            //ItemControl.Template = Application.Current.Resources["MetroLineTemplate"] as ControlTemplate;
        }

        public LocationCollection Locations { get; set; }
        public SolidColorBrush Color { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Name.Equals(((MetroLine)obj).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
