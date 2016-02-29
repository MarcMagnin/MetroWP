using System.Windows;
using System.Windows.Controls;

namespace METRO.Model
{
    public class NextStation: Station
    {

        public NextStation()
        {
            ItemControlChanged += MyItemControlChanged;
        }

        private void MyItemControlChanged(object sender, System.EventArgs e)
        {
            ItemControl.Template = Application.Current.Resources["NextStationTemplate"] as ControlTemplate;
            Deployment.Current.Dispatcher.BeginInvoke(() => VisualStateManager.GoToState(ItemControl, "Normal", true));
        }

    }


}