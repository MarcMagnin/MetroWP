using System;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using METRO.Controls;
using METRO.Helpers;
using METRO.Model;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Platform;
using Microsoft.Phone.Reactive;

namespace METRO.ViewModel
{
    public class ItineraryViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainModel;
        public bool ItineraryMode;
        public ItineraryViewModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;
           
                                                    
        }
      
          #region members

        private bool flagPreventDoubleFocusCall = false;
        private WatermarkAutoCompleteBox _fromAutoComplete;
        private string _previsousfromAutoCompleteCompleteSearch;
        public WatermarkAutoCompleteBox FromAutoComplete
        {
            get { return _fromAutoComplete; }
            set
            {
                _fromAutoComplete = value;
                // Focus du textbox TO pour saisir le lieu d'arrivé
                _fromAutoComplete.SelectionChanged += (s, e) =>
                                                          {
                                                              Debug.WriteLine("SelectionChanged"); 
                                                              if (e.AddedItems.Count > 0 && e.AddedItems[0] != null && e.AddedItems[0] is Station)
                                                              {
                                                                  if(_mainModel.IsBusy == true)
                                                                      return;
                                                                  if(flagSelectionFromButton)
                                                                      {
                                                                          flagSelectionFromButton = false;
                                                                      return;
                                                                      }
                                                                  flagPreventDoubleFocusCall = true;
                                                                  Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                  {

                                                                      Reset();
                                                                      var station = e.AddedItems[0] as Station;

                                                                      var pin = MainViewModel.PinPool.Where(p => p.DataContext == station).FirstOrDefault();
                                                                      if (pin == null) MainViewModel.PinPool.Where(p => p.DataContext == null).FirstOrDefault();
                                                                      if (pin == null)
                                                                          pin = MainViewModel.PinPool.FirstOrDefault();

                                                                      _mainModel.UnselectStations();
                                                                      _mainModel.StationsLayer.Children.Remove(pin);
                                                                      _mainModel.AddPinToStationLayer(pin, station);
                                                                      _mainModel.FocusStation(pin);


                                                                      // Léger décallage afin de voir la station derrière le clavier
                                                                      var loc = new Location();
                                                                      loc.Latitude = station.Location.Latitude - 0.004;
                                                                      loc.Longitude = station.Location.Longitude;

                                                                      _mainModel.Map.AnimationLevel = AnimationLevel.None;
                                                                      _mainModel.Map.SetView(loc, 15);
                                                                      _mainModel.Map.AnimationLevel = AnimationLevel.Full;
                                                                      
                                                                  });
                                                                
                                                                   
                                                              }
                                                              

                                                          };
                _fromAutoComplete.KeyUp += (s, e) =>
                {
                    Debug.WriteLine("KeyUp"); 
                    if (e.Key == Key.Enter)
                    {
                          ToAutoComplete.Focus();
                    }
                };

                _fromAutoComplete.GotFocus += (s, e) =>
                {
                    if (flagPreventDoubleFocusCall)
                    {
                        flagPreventDoubleFocusCall = false;
                        ToAutoComplete.Focus();
                        return;
                    }
                    Debug.WriteLine("GotFocus");
                    _fromAutoComplete.TextBox.SelectAll();
                };
        

                _fromAutoComplete.Populating += (s, e) =>
                {
                    if (flagPreventDoubleFocusCall)
                    {
                        return;
                    }
                    Debug.WriteLine("Populating");
                    if (_previsousfromAutoCompleteCompleteSearch == e.Parameter)
                        return;
                    _previsousfromAutoCompleteCompleteSearch = e.Parameter;
                    e.Cancel = true;
                    var searchText = e.Parameter.RemoveAccents();
                    _fromAutoComplete.ItemsSource= _mainModel.DistincStations.Where(i => i.NormalizedName.Contains(searchText)).Take(5).ToList();
                   // _fromAutoComplete.PopulateComplete();
                };
            }
        }


        private WatermarkAutoCompleteBox _toAutoComplete;
        private string _previsousToAutoCompleteCompleteSearch;
        public WatermarkAutoCompleteBox ToAutoComplete
        {
            get { return _toAutoComplete; }
            set
            {
                _toAutoComplete = value;

                _toAutoComplete.SelectionChanged += (s, e) => {
                    Debug.WriteLine("SelectionChanged (ToAutoComplete)"); 
                    if (_mainModel.IsBusy == true)
                        return;

                    if (e.AddedItems.Count > 0 && e.AddedItems[0] != null && e.AddedItems[0] is Station && !string.IsNullOrWhiteSpace(_fromAutoComplete.Text))
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            SearchItinerary();
                        });
                    }
                };
                _toAutoComplete.KeyUp += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        SearchItinerary();
                    }
                };


                _toAutoComplete.GotFocus += (s, e) => { Debug.WriteLine("GotFocus (ToAutoComplete)"); _toAutoComplete.TextBox.SelectAll(); };

                _toAutoComplete.Populating += (s, e) =>
                {
                    Debug.WriteLine("Populating (ToAutoComplete)");
                    if (_previsousToAutoCompleteCompleteSearch == e.Parameter)
                        return;
                    _previsousToAutoCompleteCompleteSearch = e.Parameter;
                    e.Cancel = true;
                    var searchText = e.Parameter.RemoveAccents();
                    _toAutoComplete.ItemsSource = _mainModel.DistincStations.Where(i => i.NormalizedName.Contains(searchText)).Take(5).ToList();
                  //  _toAutoComplete.PopulateComplete();
                };
            }
        }

        private bool flagSelectionFromButton = false;
        private RelayCommand setFromCommand;
        public RelayCommand SetFromCommand
        {
            get
            {
                return setFromCommand ?? (setFromCommand = new RelayCommand(() =>
                {
                   if(_mainModel.SelectedStation != null)
                   {
                       flagSelectionFromButton = true;
                       FromItem = _mainModel.SelectedStation;
                   }
                   else if (_mainModel.AddressViewModel.POILookUpSelected)
                   {
                       
                        if (_mainModel.IsBusy)
                            return;

                        flagSelectionFromButton = true;
                        Station selectedStation = null;
                        var minDist = double.MaxValue;
                        foreach (var station in ShortestPath.Stations)
                        {
                            var dist = station.Location.GetDistanceTo(_mainModel.AddressViewModel.POILookup.Location);
                            if (minDist > dist)
                            {
                                selectedStation = station;
                                minDist = dist;
                            }
                        }
                        FromItem = selectedStation;
                        // On détermine la station la plus proche
                        FromLocation = selectedStation.Location;
                        }
                }));
            }
        }
        private RelayCommand setToCommand;
        public RelayCommand SetToCommand
        {
            get
            {
                return setToCommand ?? (setToCommand = new RelayCommand(() =>
                {
                    if (_mainModel.SelectedStation != null)
                    {
                        if( _mainModel.IsBusy )
                            return;

                        ToItem = _mainModel.SelectedStation;
                        // lancement direct de l'itinéraire depuis la localisation de l'utilisateur
                        if(fromItem == null)
                        {
                            _mainModel.IsBusy = true;
                            App.BusyMessage.Message = "Recherche de votre localisation...";
                            App.BusyMessage.IsOpen = true;
                            

                            new GeoWrapper().Location().ObserveOnDispatcher().Subscribe(result =>
                            {
                               
                                _mainModel.GPSViewModel.UserLocation = result;
                                _mainModel.GPSViewModel.UserLocationVisibility= Visibility.Visible;
                                var minDist = double.MaxValue;
                                Station selectedStation = null;
                                // TODO améliorer la recherche en proposant plusieurs station à proximité
                                foreach (var station in ShortestPath.Stations)
                                {
                                    var dist = station.Location.GetDistanceTo(result);
                                    if( minDist>dist)
                                    {
                                        selectedStation = station;
                                        minDist = dist;
                                    }
                                }
                                flagSelectionFromButton = true;
                                FromItem = selectedStation;
                                SearchItinerary();
                            }, (err)=>{}, ()=>
                                              {
                                                  _mainModel.IsBusy = false;
                                                  App.BusyMessage.IsOpen = false;

                                              });
                        }
                        
                    }
                    else if (_mainModel.AddressViewModel.POILookUpSelected)
                    {
                        if (_mainModel.IsBusy)
                            return;
                        // On détermine la station la plus proche
                        ToLocation = _mainModel.AddressViewModel.POILookup.Location;
                        Station selectedStation = null;
                        var minDist = double.MaxValue;
                        foreach (var station in ShortestPath.Stations)
                        {
                            var dist = station.Location.GetDistanceTo(_mainModel.AddressViewModel.POILookup.Location);
                            if (minDist > dist)
                            {
                                selectedStation = station;
                                minDist = dist;
                            }
                        }
                        ToItem = selectedStation;


                        // lancement direct de l'itinéraire depuis la localisation de l'utilisateur
                        if (fromItem == null)
                        {
                            _mainModel.IsBusy = true;
                            App.BusyMessage.Message = "Recherche de votre localisation...";
                            App.BusyMessage.IsOpen = true;


                            new GeoWrapper().Location().ObserveOnDispatcher().Subscribe(result =>
                            {

                                _mainModel.GPSViewModel.UserLocation = result;
                                _mainModel.GPSViewModel.UserLocationVisibility = Visibility.Visible;
                                minDist = double.MaxValue;
                                selectedStation = null;
                                // TODO améliorer la recherche en proposant plusieurs station à proximité
                                foreach (var station in ShortestPath.Stations)
                                {
                                    var dist = station.Location.GetDistanceTo(result);
                                    if (minDist > dist)
                                    {
                                        selectedStation = station;
                                        minDist = dist;
                                    }
                                }
                                flagSelectionFromButton = true;
                                FromItem = selectedStation;
                                SearchItinerary();
                            }, err => { }, () =>
                            {
                                _mainModel.IsBusy = false;
                                App.BusyMessage.IsOpen = false;

                            });
                        }











                    }
                }));
            }
        }

       


        private RelayCommand showHideItineraryCommand;
        public RelayCommand ShowHideItineraryCommand
        {
            get
            {
                return showHideItineraryCommand ?? (showHideItineraryCommand = new RelayCommand(() =>
                {
                    if (_mainModel.CurrentVisualState != "ItineraryState")
                    {
                        _mainModel.CurrentVisualState = "ShowBackground";
                        _mainModel.CurrentVisualState = "ItineraryState";
                    }
                    else
                    {
                        _mainModel.CurrentVisualState = "HideBackground";
                        _mainModel.CurrentVisualState = "NormalState";
                        _mainModel.Map.Focus();
                    }
                }));
            }
        }

       

          private string fromText;
          public string FromText
          {
              get { return fromText; }
              set
              {
                  if (fromText != value)
                  {
                      fromText = value;
                      RaisePropertyChanged("FromText");
                  }
              }
          }
          private string toText;
          public string ToText
          {
              get { return toText; }
              set
              {
                  if (toText != value)
                  {
                      toText = value;
                      RaisePropertyChanged("ToText");
                  }
              }
          }

          private MapItem fromItem;
          public MapItem FromItem
          {
              get { return fromItem; }
              set
              {
                  if (fromItem != value)
                  {
                     
                      fromItem = value;
                      var station = fromItem as Station;
                      if (station != null) FromLocation = station.Location;
                     
                      RaisePropertyChanged("FromItem");
                  }
              }
          }      
        private MapItem toItem;
          public MapItem ToItem
          {
              get { return toItem; }
              set
              {
                  if (toItem != value)
                  {
                      toItem = value;
                      var station = toItem as Station;
                      if (station != null) ToLocation = station.Location;
                      RaisePropertyChanged("ToItem");
                  }
              }
          }

          private GeoCoordinate toLocation;
          public GeoCoordinate ToLocation
          {
              get { return toLocation; }
              set
              {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _mainModel.CurrentVisualState = "HideToPin";
                         
                     
                    toLocation = value;
                    RaisePropertyChanged("ToLocation");
                    });
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _mainModel.CurrentVisualState = "ShowToPin";

                    });
              }
          }   
        
        private GeoCoordinate fromLocation;
          public GeoCoordinate FromLocation
          {
              get { return fromLocation; }
              set
              {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _mainModel.CurrentVisualState = "HideFromPin";
                        fromLocation = value;
                        RaisePropertyChanged("FromLocation");
                    });
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _mainModel.CurrentVisualState = "ShowFromPin";
                    });
              }
          }


        //private readonly ObservableCollection<Itinerary> itineraries = new ObservableCollection<Itinerary>();
        //public ObservableCollection<Itinerary> Itineraries
        //{
        //    get { return itineraries; }
        //}


        private readonly ObservableCollection<Station> stationsItinerary = new ObservableCollection<Station>();
        public ObservableCollection<Station> StationsItinerary
        {
            get { return stationsItinerary; }
        }


        #endregion members

        #region methodes

        public void Reset()
        {
            ToItem = null;
            ToText = string.Empty;
            _mainModel.CurrentVisualState = "HideItineraryInfo";
            _mainModel.CurrentVisualState = "HideToPin";
            _mainModel.CurrentVisualState = "HideFromPin";

            stationsItinerary.Clear();
            foreach (var line in _mainModel.MetroLinesControl)
            {
                line.Opacity = 1;
            }
            ItineraryMode = false;
        }

        public override void Cleanup()
        {
            base.Cleanup();

            if(GeoWrapper != null){
                GeoWrapper.Dispose();
                GeoWrapper = null;
                _mainModel.IsBusy = false;
                App.BusyMessage.IsOpen = false;
            }

            FromItem = null;
            ToItem = null;

            _previsousfromAutoCompleteCompleteSearch = null;
            _previsousToAutoCompleteCompleteSearch = null;

            FromText = string.Empty;
            ToText = string.Empty;
            _mainModel.CurrentVisualState = "HideItineraryInfo";
            _mainModel.CurrentVisualState = "HideToPin";
            _mainModel.CurrentVisualState = "HideFromPin";
           
            
            stationsItinerary.Clear();
            foreach (var line in _mainModel.MetroLinesControl)
            {
                line.Opacity = 1;
            }
            ItineraryMode = false;


            _mainModel.SetStationLabelOpacity();

            _mainModel.UnselectStations();

            if (_mainModel.Map.TargetZoomLevel < 14)
            {
                _mainModel.CurrentVisualState = "HideMetroStations";
              
            }

        }

       private void GoForItinerary()
       {
           ItineraryMode = true;
           _mainModel.UnselectStations();
           // Run dijkstra
           try
           {
               int minPath = int.MaxValue;
               Dijkstra goodDijk = null;
               // Pour chaque station de départ potentielle, on détermine la station de départ la meilleur
               foreach (var station in ShortestPath.Stations.Where(s => s.Name == fromItem.Name && s.Id != fromItem.Id))
               {
                   var dijk = new Dijkstra(ShortestPath.G, station.Id);
                   var length = ShortestPath.GetMinimumPath(station.Id, toItem.Id, dijk.path).Length;
                   if (length < minPath)
                   {
                       minPath = length;
                       goodDijk = dijk;
                   }
               }
               if (goodDijk == null)
               {
                   goodDijk = new Dijkstra(ShortestPath.G, fromItem.Id);
               }


               var dist = goodDijk.dist;
               var path = goodDijk.path;
               var minimumPath = ShortestPath.GetMinimumPath(fromItem.Id, toItem.Id, path);



               StartStation startStation = null;
               Station previousStation = null;
               int startIndex = 0;
               var firstNextStation = ShortestPath.Stations[minimumPath[0]];
               // Si la station suivante fait partie du meme groupe de station (ie : Chatelet 1 et Chatelet 11)
               // On démarre avec la station suivante
               if ((fromItem as Station).Name == firstNextStation.Name)
               {
                   previousStation = firstNextStation;
                   startIndex = 1;
                   startStation = new StartStation
                   {
                       Id = firstNextStation.Id,
                       Name =firstNextStation.Name,
                       CurrentLine = firstNextStation.CurrentLine,
                       PrendreLaLigne = "Prendre la ligne ",
                       Location = firstNextStation.Location
                   };

               }
               else
               {
                   previousStation = (fromItem as Station);
                   startStation = new StartStation
                   {
                       Id = (fromItem as Station).Id,
                       Name = (fromItem as Station).Name,
                       CurrentLine = (fromItem as Station).CurrentLine,
                       PrendreLaLigne = "Prendre la ligne ",
                       Location = (fromItem as Station).Location
                   };
               }

               stationsItinerary.Add(startStation);


               // Déterminaison de la direction
               var nextstationn = ShortestPath.Stations[minimumPath[startIndex]];
               if (nextstationn.PreviousStation == previousStation)
                   startStation.Direction +=  previousStation.BottomStation;
               else
                   startStation.Direction +=  previousStation.TopStation; 

               for (var i = startIndex; i < minimumPath.Length; i++)
               {

                   var nextstation = ShortestPath.Stations[minimumPath[i]];



                   if (nextstation.CurrentLine == previousStation.CurrentLine)
                   {

                       // Si la dernière station ne fait partie du meme groupe, on l'ajoute
                       if (previousStation.Name != nextstation.Name)
                       {
                           stationsItinerary.Add(new NextStation
                           {
                               Id = nextstation.Id,
                               CurrentLine = nextstation.CurrentLine,
                               Name = nextstation.Name,
                               Location = nextstation.Location
                           });
                       }


                       // itinerary.StationCount++;
                   }
                   else if (nextstation.Name != (toItem as Station).Name)
                   {
                       // On retire la station précédente car elle a le meme nom, seul le numéro de ligne change
                       stationsItinerary.RemoveAt(stationsItinerary.Count - 1);
                       // itinerary.StationCount--;

                       var changeStation = new ChangeStation
                       {
                           Id = nextstation.Id,
                           Name = nextstation.Name,
                           CurrentLine = nextstation.CurrentLine,
                           PrendreLaLigne =
                               "Prendre la ligne ",
                           Location = nextstation.Location,
                       };

                       // Détermination de la direction
                       if (i + 1 < minimumPath.Length)
                       {
                           var stationAfterNextStation = ShortestPath.Stations[minimumPath[i + 1]];
                           if (stationAfterNextStation.PreviousStation == nextstation)
                               changeStation.Direction += nextstation.BottomStation;
                           else
                               changeStation.Direction +=  nextstation.TopStation;

                           stationsItinerary.Add(changeStation);
                       }
                       //itinerary = new Itinerary
                       //                {
                       //                    StationDepart = nextstation.Name,
                       //                    Ligne = nextstation.CurrentLine
                       //                };
                       //Itineraries.Add(itinerary);
                   }

                   previousStation = nextstation;





               }

              

               // On ajoute la dernière station du type "EndStation"
               // Si la dernière station fait partie du meme groupe, on a retire
               if (previousStation.Name == (toItem as Station).Name)
               {
                   stationsItinerary.RemoveAt(StationsItinerary.Count - 1);
               }
               stationsItinerary.Add(new EndStation
               {
                   Id = toItem.Id,
                   Name = toItem.Name,
                   Location = (toItem as Station).Location
               });

               // Permet d'éviter d'avoir les station intermédiaires ayant des z-index supérieur
               for (int index = 0; index < stationsItinerary.Count; index++)
               {
                   var station = stationsItinerary[index];
                   if (station is StartStation || station is ChangeStation)
                   {
                       stationsItinerary.Remove(station);
                       stationsItinerary.Add(station);
                   }
               }

               var locations = stationsItinerary.Select(station => station.Location).ToList();




               _mainModel.Map.Focus();
               _mainModel.Map.AnimationLevel = AnimationLevel.None;
               _mainModel.Map.SetView(LocationRect.CreateLocationRect(locations));
               _mainModel.Map.ZoomLevel = _mainModel.Map.ZoomLevel-1;
               _mainModel.Map.AnimationLevel = AnimationLevel.Full;

               _mainModel.CurrentVisualState = "HideBackground";
               _mainModel.CurrentVisualState = "NormalState";
               _mainModel.CurrentVisualState = "ShowItineraryInfo";

               var stationLines = stationsItinerary.Select(s => s.CurrentLine).Distinct().ToList();

               foreach (var line in _mainModel.MetroLinesControl)
               {
                   line.Opacity = 1;
               }
               foreach (var line in _mainModel.MetroLinesControl)
               {
                   if (!stationLines.Contains((line.DataContext as MetroLine).Name.Replace("Ligne ", "")))
                       line.Opacity = 0.3;
               }


           }
           catch (ArgumentException err)
           {
               MessageBox.Show(err.Message);
           }finally
           {
               _mainModel.IsBusy = false;
           }
       }

        private void SearchItinerary()
        {
           // Itineraries.Clear();
            stationsItinerary.Clear();

            if(fromItem == null)
            {
                fromItem = ShortestPath.Stations.Where(s => s.Name == _fromAutoComplete.Text.Trim()).FirstOrDefault();
            }
            if (toItem == null)
            {
                toItem = ShortestPath.Stations.Where(s => s.Name == _toAutoComplete.Text.Trim()).FirstOrDefault();
            }

            if(fromItem == toItem)
            {
                return;
            }
            // Cas ou les stations ont bien été saisies
            if (fromItem != null && toItem != null)
            {
                GoForItinerary();
            }
            else
            {
                _mainModel.IsBusy = true;
                App.BusyMessage.Message = "Recherche de votre itinéraire...";
                App.BusyMessage.IsOpen = true;


                if (fromItem == null && toItem == null)
                {
                    ResolveToAndFrom();
                }
                else if (toItem == null)
                {
                    // Si des adresses ont été saisie, tentative de résolution des adresses
                    new GeoWrapper().Adresse(toText)
                        .ObserveOnDispatcher()
                        .Subscribe(result =>
                                       {
                                           if (result == null)
                                           {
                                               MessageBox.Show("Impossible de résoudre l'adresse d'arrivée.");
                                               _mainModel.IsBusy = false;
                                               App.BusyMessage.IsOpen = false;
                                               return;
                                           }
                                           ToItem = GetNearestStation(result);
                                           GoForItinerary();
                                       });
                }
                else if (fromItem == null)
                {
                    // Si des adresses ont été saisie, tentative de résolution des adresses
                    new GeoWrapper().Adresse(toText)
                        .ObserveOnDispatcher()
                        .Subscribe(result =>
                        {
                            if (result == null)
                            {
                                MessageBox.Show("Impossible de résoudre l'adresse de départ.");
                                _mainModel.IsBusy = false;
                                App.BusyMessage.IsOpen = false;
                                return;
                            }
                            FromItem = GetNearestStation(result);
                            GoForItinerary();
                        });
                }
            }
        }



        public IDisposable GeoWrapper;

        private void ResolveToAndFrom()
        {
            if (GeoWrapper != null)
                GeoWrapper.Dispose();
            // Si des adresses ont été saisie, tentative de résolution des adresses
            GeoWrapper = new GeoWrapper().Adresse(fromText).Zip(new GeoWrapper().Adresse(toText), (first, second) => new[] { first, second })
                .ObserveOnDispatcher()
                .Subscribe(result =>
                {
                    if (result[0] == null)
                    {
                        MessageBox.Show("Impossible de résoudre l'adresse de départ");
                        _mainModel.IsBusy = false;
                        App.BusyMessage.IsOpen = false;
                        return;

                    }
                    else
                    {
                        Debug.WriteLine(result[0]);
                    }

                    if (result[1] == null)
                    {
                        MessageBox.Show("Impossible de résoudre l'adresse d'arrivée");
                        _mainModel.IsBusy = false;
                        App.BusyMessage.IsOpen = false;
                        return;
                    }
                    else
                    {
                        Debug.WriteLine(result[1]);
                    }


                    FromItem = GetNearestStation(result[0]);
                    ToItem = GetNearestStation(result[1]);
                    GoForItinerary();
                });

        }
        private Station GetNearestStation (GeoCoordinate coord)
        {
            var minDist = double.MaxValue;
            Station selectedStation = null;
            foreach (var station in ShortestPath.Stations)
            {
                var dist = station.Location.GetDistanceTo(coord);
                if (minDist > dist)
                {
                    selectedStation = station;
                    minDist = dist;
                }
            }
            return selectedStation;
        }

        #endregion
    }
}
