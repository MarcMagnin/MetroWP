using System;
using System.Device.Location;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Shell;

namespace METRO.ViewModel
{
    public class GPSViewModel : ViewModelBase
    {
        private MainViewModel _mainModel;
        public GPSViewModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;
            Watcher.StatusChanged += watcher_StatusChanged;
        }
        private string gpsStatus;
        public string GPSStatus
        {
            get { return gpsStatus; }
            set
            {
                if (gpsStatus != value)
                {
                    gpsStatus = value;
                    RaisePropertyChanged("GPSStatus");
                }
            }
        }

        private Visibility _userLocationVisibility = Visibility.Collapsed;
        public Visibility UserLocationVisibility
        {
            get { return _userLocationVisibility; }
            set
            {
                if (_userLocationVisibility != value)
                {
                    _userLocationVisibility = value;
                    RaisePropertyChanged("UserLocationVisibility");
                }
            }
        }
        private GeoCoordinateWatcher watcher;
        public GeoCoordinateWatcher Watcher
        {
            get
            {
                return watcher ?? (watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default)
                                                 {
                                                     MovementThreshold = 10,

                                                 });
            }
        }

        private DispatcherTimer _timeout = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        private ApplicationBarIconButton _senderButton;
        private RelayCommand<ApplicationBarIconButton> _showMyLocationCommand;
        public RelayCommand<ApplicationBarIconButton> ShowMyLocationCommand
        {
            get
            {
                return _showMyLocationCommand ?? (_showMyLocationCommand = new RelayCommand<ApplicationBarIconButton>((button) =>
                {
                    _senderButton = button;
                    _senderButton.IsEnabled = false;
                    if (!CheckPrivacyPolicy())
                        return;

                    // CurrentVisualState = VisualStates.HideSearchPanel;
                    //CurrentVisualState = VisualStates.HideLoginPanel;
                    _timeout.Tick += _timeout_Tick;
                    Watcher.Start();

                }));
            }
        }

        /// <summary>
        /// See 2.10.5 of Windows Phone 7 Application Cerficiation Requirements
        /// </summary>
        public bool IsLocationServiceOpen
        {
            get
            {

              //  return App.IsLocationServiceOpen;
                return true;

            }
            set
            {
                //App.IsLocationServiceOpen = value;
                RaisePropertyChanged("IsLocationServiceOpen");
            }
        }
        void _timeout_Tick(object sender, EventArgs e)
        {
            _timeout.Stop();
            _senderButton.IsEnabled = true;

            if (GPSStatus == "Paused")
                return;

            watcher.Stop();
            GPSStatus = "";
            _mainModel.IsBusy = false;
            MessageBox.Show("Désolé, nous sommes dans l'impossibilité de trouver votre localisation, merci de vérifier votre couverture réseau et retenter l'opération.");
        }
        private bool CheckPrivacyPolicy()
        {

            if (!IsLocationServiceOpen)
            {
                if (MessageBox.Show("Votre localisation va être envoyée aux services de CBS Outdoor, souhaitez-vous partager votre localisation ? " +
                    "Sachez que votre localisation ne sera ni conservée, ni utilisée à aucune autre fin que celle de déterminer le dispositif le plus proche de vous. " +
                    "Vous pouvez à tout moment revenir sur votre choix dans les paramètres de l'application et accéder à la déclaration de confidentialité.", "Autoriser le service de localisation ?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    IsLocationServiceOpen = true;
                }
                else
                {
                    IsLocationServiceOpen = false;
                }

            }
            return IsLocationServiceOpen;
        }

        /// <summary>
        /// the location of the GPS that represente the user location
        /// </summary>
        private GeoCoordinate userLocation;
        public GeoCoordinate UserLocation
        {
            get { return userLocation; }
            set
            {
                if (userLocation != value)
                {
                    userLocation = value;
                    RaisePropertyChanged("UserLocation");
                }
            }
        }
        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                
                switch (e.Status)
                {
                    case GeoPositionStatus.Initializing:
                        _timeout.Start();
                        _mainModel.IsBusy = true;
                        GPSStatus = "Service de localisation : Initialisation";
                        break;

                    case GeoPositionStatus.NoData:
                        GPSStatus = "Localisation en cours...";
                        break;
                    case GeoPositionStatus.Ready:
                        
                        UserLocation = watcher.Position.Location;
                        UserLocationVisibility = Visibility.Visible;
                        if (IsLocationServiceOpen)
                        {
                           _mainModel.Map.Center = UserLocation;
                           _mainModel.Map.ZoomLevel = 15;

                        }
                        _mainModel.IsBusy = false;
                        GPSStatus = "Paused";
                        
                        watcher.Stop();
                        _timeout.Stop();
                        _senderButton.IsEnabled = true;

                        break;
                    case GeoPositionStatus.Disabled:
                        _timeout.Stop();
                        _senderButton.IsEnabled = true;
                        _mainModel.IsBusy = false;
                        break;
                }
            });
        }



    }
}
