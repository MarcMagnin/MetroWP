using System.Windows;
using System.Windows.Controls;

namespace METRO.Model
{
    public class ChangeStation: Station
    {

        public ChangeStation()
        {
            ItemControlChanged += MyItemControlChanged;
        }

        private void MyItemControlChanged(object sender, System.EventArgs e)
        {
            ItemControl.Template = Application.Current.Resources["ChangeStationTemplate"] as ControlTemplate;
            Deployment.Current.Dispatcher.BeginInvoke(() => VisualStateManager.GoToState(ItemControl,"Normal", true));
                              
        }
        private string direction;
        public string Direction
        {
            get { return direction; }
            set
            {
                if (direction != value)
                {
                    direction = value;
                    RaisePropertyChanged("Direction");
                }
            }
        }

        private string prendreLaLigne;
        public string PrendreLaLigne
        {
            get { return prendreLaLigne; }
            set
            {
                if (prendreLaLigne != value)
                {
                    prendreLaLigne = value;
                    RaisePropertyChanged("PrendreLaLigne");
                }
            }
        }
   
    }
}