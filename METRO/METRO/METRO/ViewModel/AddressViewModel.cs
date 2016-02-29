using System;
using System.Device.Location;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using METRO.GeocodeService;
using METRO.SystemRessources;
using Microsoft.Expression.Interactivity.Layout;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Platform;
using Microsoft.Phone.Reactive;

namespace METRO.ViewModel
{
    public class AddressViewModel : ViewModelBase
    {

        private MainViewModel _mainModel;
        public AddressViewModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;

        }

        public const string ActionPOIContentName = "ActionPOIContent";
        private string actionPOIContent;
        public string ActionPOIContent
        {
            get
            {
                return actionPOIContent;
            }
            set
            {
                if (actionPOIContent == value)
                {
                    return;
                }
                actionPOIContent = value;
                RaisePropertyChanged(ActionPOIContentName);
            }
        }


        public const string ActionPOILocationName = "ActionPOILocation";
        private GeoCoordinate actionPOILocation;
        public GeoCoordinate ActionPOILocation
        {
            get
            {
                return actionPOILocation;
            }
            set
            {
                if (actionPOILocation == value)
                {
                    return;
                }
                actionPOILocation = value;
                RaisePropertyChanged(ActionPOILocationName);
            }
        }

        public Pushpin POILookup{ get ; set; }

        public bool POILookUpSelected;

        public GeocodeServiceClient GeoCodeService;
        private FluidMoveBehavior behavior;

        private RelayCommand poiLookUpManipStartedCommand;
        public RelayCommand PoiLookUpManipStartedCommand
        {
            get
            {
                return poiLookUpManipStartedCommand ?? (poiLookUpManipStartedCommand = new RelayCommand(() =>
                {
                    POILookup.Foreground = PhoneBrushes.PhoneBackgroundBrush;
                    POILookup.Background = PhoneBrushes.PhoneForegroundBrush;
                }));
            }
        }

        private RelayCommand poiLookUpManipCompletedCommand;
        public RelayCommand PoiLookUpManipCompletedCommand
        {
            get
            {
                return poiLookUpManipCompletedCommand ?? (poiLookUpManipCompletedCommand = new RelayCommand(() =>
                {
                    POILookup.Foreground = PhoneBrushes.PhoneForegroundBrush;
                    POILookup.Background = PhoneBrushes.PhoneBackgroundBrush;
                    if (_mainModel.ItineraryViewModel.ItineraryMode)
                        return;
                    _mainModel.UnselectStations();
                    VisualStateManager.GoToState(POILookup, "Selected", false);
                    _mainModel.CurrentVisualState = "SelectedState";
                    POILookUpSelected = true;
                }));
            }
        }

        public override void Cleanup()
        {
            _mainModel.IsBusy = false;
            GeoCodeService.Abort();
            GeoCodeService = null;
        }


        private RelayCommand<KeyEventArgs> searchKeyUpCommand;
        public RelayCommand<KeyEventArgs> SearchKeyUpCommand
        {
            get
            {
                return searchKeyUpCommand ?? (searchKeyUpCommand = new RelayCommand<KeyEventArgs>(e =>
                {
                  if (e.Key != Key.Enter || string.IsNullOrEmpty(_mainModel.MetroAutoComplete.Text.Trim()))
                return;

                    // avant de rechercher une adresse, on lance la recherche dans les stations

            _mainModel.IsBusy = true;

            var request = new GeocodeRequest()
            {
                Query = _mainModel.MetroAutoComplete.Text,
                //Address =  new Address{ 

                //    AddressLine = TBSearchAddress.Text,
                //    CountryRegion = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                //},
                Credentials = new Credentials
                {
                    ApplicationId = App.MapId
                }
            };

            if (GeoCodeService != null)
                GeoCodeService.Abort();
            GeoCodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            GeoCodeService.GeocodeCompleted += (s, evt) =>
            {
                if (behavior == null)
                    behavior = System.Windows.Interactivity.Interaction.GetBehaviors(POILookup)[0] as FluidMoveBehavior;
                behavior.IsActive = false;
                POILookup.IsHitTestVisible = true;
                POILookup.Opacity = 0.8;
                try
                {
                    // if there is any address     
                    if (evt.Result.Results.Count > 0)
                    {
                        // use the first one 
                        ActionPOILocation = new GeoCoordinate(evt.Result.Results[0].Locations[0].Latitude, evt.Result.Results[0].Locations[0].Longitude);
                        ActionPOIContent = evt.Result.Results[0].Address.FormattedAddress;
                        _mainModel.Map.SetView(ActionPOILocation, 14);
                        _mainModel.CurrentVisualState = "HideBackground";
                        _mainModel.CurrentVisualState = "NormalState";
                        _mainModel.Map.Focus();
                    }
                    // else show invalid position  
                    else
                    {
                        // vm.ActionPOIContent = "Invalid";
                        MessageBox.Show("Impossible de trouver cette adresse");
                    }
                }
                catch
                {
                    //MessageBox.Show("Impossible de déterminer la localisation recherchée.");
                    // vm.ActionPOIContent = "?";
                }
                finally
                {
                    _mainModel.IsBusy = false;
                }

            };
            // make the request   
            GeoCodeService.GeocodeAsync(request);
                    }));
            }
        }

        private RelayCommand<GestureEventArgs> mapHoldCommand;
        public RelayCommand<GestureEventArgs> MapHoldCommand
        {
            get
            {
                return mapHoldCommand ?? (mapHoldCommand = new RelayCommand<GestureEventArgs>(args =>
                {
                    // get the geolocation of that position  
                    POILookup.IsHitTestVisible = false;

                    POILookup.Opacity = 0;
                    if(behavior == null)
                        behavior = System.Windows.Interactivity.Interaction.GetBehaviors(POILookup)[0] as FluidMoveBehavior;
                    behavior.Duration = new TimeSpan(0);
                    behavior.IsActive = true;

                    if (ActionPOILocation != null)
                    {
                        var p = _mainModel.Map.LocationToViewportPoint(ActionPOILocation);
                        p.X += 1;
                        p.Y += 1;
                        ActionPOILocation = _mainModel.Map.ViewportPointToLocation(p);
                    }

                    Observable.Start(() => { /* Trick for fluid behavior */ })
                    .ObserveOnDispatcher()
                    .Do(u =>
                    {
                        POILookup.Opacity = 0.8;
                        behavior.Duration = TimeSpan.FromSeconds(0.6);
                        ActionPOILocation = _mainModel.Map.ViewportPointToLocation(args.GetPosition( _mainModel.Map));
                        //App.BusyMessage.Message = "Recherche d'adresse...";
                        ActionPOIContent = "Recherche...";
                       // _mainModel.IsBusy = true;
                        POILookup.IsHitTestVisible = true;
                    })
                    .ObserveOn(Scheduler.ThreadPool)
                    .Do(f =>
                    {
                        var request = new ReverseGeocodeRequest()
                        {
                            Location = new Location()
                            {
                                Latitude = ActionPOILocation.Latitude,
                                Longitude = ActionPOILocation.Longitude
                            },
                            Credentials = new Credentials()
                            {
                                ApplicationId = App.MapId
                            }
                        };

                        if (GeoCodeService != null)
                            GeoCodeService.Abort();
                        GeoCodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
                        GeoCodeService.ReverseGeocodeCompleted += (s, evt) =>
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                // Happen on dispatcher
                                behavior.IsActive = false;

                                try
                                {
                                    // if there is any address     
                                    if (evt.Result.Results.Count > 0)
                                        // use the first one       
                                        ActionPOIContent = evt.Result.Results[0].Address.FormattedAddress;
                                    // else show invalid position  
                                    else
                                        ActionPOIContent = "Impossible de trouver l'adresse.";
                                }
                                catch (CommunicationObjectAbortedException)
                                {
                                    // Nothing to do
                                }
                                catch
                                {
                                    ActionPOIContent = "Impossible de trouver l'adresse actuellement.";
                                }
                          //      _mainModel.IsBusy = false;
                            });
                        };

                        // make the request   
                        GeoCodeService.ReverseGeocodeAsync(request);

                    }).Subscribe();
                }));
            }
        }
    }
}
