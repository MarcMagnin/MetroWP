using System.Windows;
using System.Windows.Controls;


namespace METRO.Model
{
    public class EndStation : Station
    {

        public EndStation()
        {
            ItemControlChanged += MyItemControlChanged;
        }

        private void MyItemControlChanged(object sender, System.EventArgs e)
        {
            ItemControl.Template = Application.Current.Resources["EndStationTemplate"] as ControlTemplate;
            Deployment.Current.Dispatcher.BeginInvoke(() => VisualStateManager.GoToState(ItemControl,"Normal", true));
                              
        }
    }
}