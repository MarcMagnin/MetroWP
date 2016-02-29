using Microsoft.Phone.Tasks;

namespace METRO
{
    public partial class Credits
    {
        public Credits()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try{
            var marketplaceDetailTask = new MarketplaceDetailTask {ContentType = MarketplaceContentType.Applications};
            marketplaceDetailTask.Show();
            }catch{}
            
        }

        private void HyperlinkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
              try{
                var emailComposeTask = new EmailComposeTask {To = "marc.magnin@gmail.com"};
                emailComposeTask.Show();
              }
              catch { }
        }
    }
}
