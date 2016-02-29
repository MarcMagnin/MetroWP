using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using METRO.Model;
using METRO.Helpers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Reactive;

namespace METRO.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region ctor
        public MainViewModel()
        {
            
       



            TiltEffect.TiltableItems.Add(typeof(Pushpin));

            // Application.Current.Host.Settings.EnableRedrawRegions = true;
            LoadKML();
            for (int i = 0; i < MaxPinPool; i++)
            {
                var pin = new Pushpin
                {
                   // CacheMode = new BitmapCache(),
                    DataContext = null,
                    PositionOrigin = PositionOrigin.BottomRight
                    //Opacity = 0
                    //Visibility = Visibility.Collapsed
                };

                pin.Tap += pin_Tap;
                PinPool.Add(pin);

            }
          
        }

        #endregion

        private static XNamespace ns = "http://www.opengis.net/kml/2.2";



        private readonly List<MapPolyline> _metroLinesControl = new List<MapPolyline>();
        public List<MapPolyline>MetroLinesControl
        {
            get { return _metroLinesControl; }
        }


        internal void SetStationLabelOpacity()
        {
            if (Map.TargetZoomLevel > 14 && Map.TargetZoomLevel < 14.5)
            {
                StationsLabelOpacity = 0.4;
            }
            else
            {
                if (Map.TargetZoomLevel > 14.5 && Map.TargetZoomLevel < 15)
                {
                    StationsLabelOpacity = 0.7;
                }
                else
                {
                    if (Map.TargetZoomLevel >= 15)
                    {
                        StationsLabelOpacity = 1;
                    }
                }
            }
        }
        private Map _map;
        private BackgroundWorker _backgroundPinAdder = new BackgroundWorker {WorkerSupportsCancellation = true};
        public Map Map{get { return _map; }
            set { _map = value;
           
            _map.TargetViewChanged += (s, e) =>
                                          {
                                              // Return si l'utilisateur est en mode itinéraire
                                              if (ItineraryViewModel.ItineraryMode)
                                                  return;

                                              SetStationLabelOpacity();
                                            
                                         
                                              if (Map.TargetZoomLevel < 14)
                                              {
                                                  CurrentVisualState = "HideMetroStations";
                                                  // Déséléction de la station séléctionnée
                                                  if (SelectedStation != null)
                                                  {
                                                      SelectedStation.UnSelect();
                                                      SelectedStation = null;
                                                      CurrentVisualState = "NormalState";
                                                  }
                                                  

                                                    //StationsLayer.Visibility = Visibility.Collapsed;
                                                  //for (int i = 0; i < _stationsLayer.Children.Count; i++)
                                                  //{
                                                  //    var pin = _stationsLayer.Children[i] as Pushpin;
                                                    

                                                  //        _stationsLayer.Children.Remove(pin);
                                                  //        ExtendedVisualStateManager.GoToState(pin, "Normal", false);
                                                  //        VisualStateManager.GoToState(pin, "Hide", false);
                                                  //         pin.ApplyTemplate();
                                                  //        if (pin.DataContext != null)
                                                  //        {
                                                  //            var station = (pin.DataContext as Station);
                                                  //            if (station.Selected)
                                                  //            {
                                                  //                UnselectStations();
                                                  //            }
                                                  //            station.Added = false;
                                                             
                                                  //        }


                                                  //        pin.DataContext = null;

                                                  //        pin.Visibility = Visibility.Collapsed;
                                                  //        pin.Opacity = 0;
                                                    
                                                    
                                                  //}
                                              }

                                       
                                          };
                _map.ViewChangeEnd += (s, e) =>
                {
                    var stroke = 15;
                    if (_map.ZoomLevel < 12)
                        stroke = 5;
                    else if (_map.ZoomLevel > 12 && _map.ZoomLevel < 16)
                        stroke = 10;

                    foreach (var mapPolyline in MetroLinesControl)
                    {
                        mapPolyline.StrokeThickness = stroke;
                    }

                   
                    //StationsLabelVisibility = _map.ZoomLevel < 15 ? Visibility.Collapsed : Visibility.Visible;
                    if (ItineraryViewModel.ItineraryMode)
                        return;

                    if (_map.ZoomLevel < 14)
                    {
                        return;
                    }
                    CurrentVisualState = "ShowMetroStations";
                   // _stationsLayer.Visibility = Visibility.Visible;

                    var dontRemoveItems = new List<MapItem>();
                    for (int i=0; i < _stationsLayer.Children.Count;i++)
                    {
                        var pin = _stationsLayer.Children[i] as Pushpin;
                        if(!_map.TargetBoundingRectangle.Contains(pin.Location))
                        {

                            _stationsLayer.Children.Remove(pin);
                            //ExtendedVisualStateManager.GoToState(pin, "Normal", false);
                           // VisualStateManager.GoToState(pin, "Hide", false);
                           // pin.ApplyTemplate();
                            if (pin.DataContext != null)
                            {
                                _stationsLayer.Children.Remove(pin);
                                var station = (pin.DataContext as Station);
                                if (station.Selected)
                                {
                                    UnselectStations();
                                }
                                station.Added = false;
                                pin.DataContext = null;
                            }
                                

                          
                            
                            //pin.Visibility = Visibility.Collapsed;
                            //pin.Opacity = 0;
                        }else
                        {
                            dontRemoveItems.Add((MapItem)pin.DataContext);    
                        }
                    }

                    var stationToAdd = Items.Where(i => !dontRemoveItems.Contains(i) && _map.TargetBoundingRectangle.Contains(((Station)i).Location));
                    
                    _backgroundPinAdder.CancelAsync();
                    _backgroundPinAdder = new BackgroundWorker { WorkerSupportsCancellation = true };
                    _backgroundPinAdder.DoWork += (ss, ee) =>
                    {
                        var worker = ss as BackgroundWorker;
                        foreach (var station in stationToAdd)
                        {
                            if (worker.CancellationPending)
                                return;
                            Deployment.Current.Dispatcher.BeginInvoke(new AddPushPinDelegate(AddPushPin), station, worker);
                            Thread.Sleep(40);
                        }
                    };
                    _backgroundPinAdder.RunWorkerAsync(_backgroundPinAdder);
                };
            }
        }
        private delegate void AddPushPinDelegate(Station station, BackgroundWorker parentThread);


        internal void AddPinToStationLayer(Pushpin pin, Station station)
        {
            pin.DataContext = station;
            pin.Location = station.Location;
            station.Added = true;
            //station.ItemControl = pin;
            //pin.Visibility = Visibility.Visible;
            //pin.Opacity = 1;

            _stationsLayer.Children.Add(pin);
     
            // permet d'animer l'apparition de la station
            VisualStateManager.GoToState(pin, "Show", false);

            if(station.Selected)
                VisualStateManager.GoToState(pin, "Selected", true);
        }

        private void AddPushPin(Station station, BackgroundWorker parentThread)
        {
            var pin = PinPool.Where(p => p.DataContext == null).FirstOrDefault();
            if (pin == null || parentThread.CancellationPending){
                parentThread.CancelAsync();
                return;
            }
            // permet d'éviter des ajouts concurrentiels
            if(!station.Added)
                AddPinToStationLayer(pin, station);
        }

        private const int MaxPinPool = 65;
        internal static readonly List<Pushpin> PinPool = new List<Pushpin>(MaxPinPool);


        private AutoCompleteBox _metroAutoComplete;
        private string _previsousAutoCompleteSearch;
        public AutoCompleteBox MetroAutoComplete { get { return _metroAutoComplete; } set
        {
            _metroAutoComplete = value;
            _metroAutoComplete.Populating += (s, e) =>
                                                 {
                                                     if ( _previsousAutoCompleteSearch == e.Parameter)
                                                         return;
                                                     _previsousAutoCompleteSearch = e.Parameter;
                                                     e.Cancel = true;
                                                     var searchText = e.Parameter.RemoveAccents();
                                                     //var l = _itemsView.View.Cast<MapItem>().Where(i => i.NormalizedName.Contains(searchText)).Take(5).ToList();
                                                     _metroAutoComplete.ItemsSource = DistincStations.Where(i => i.NormalizedName.Contains(searchText)).Take(5).ToList();
                                                     
                                                   //  _metroAutoComplete.PopulateComplete();
                                                 };
        } }


        private MapLayer _stationsLayer;

        public MapLayer StationsLayer
        {
            get { return _stationsLayer; }
            set
            {
                _stationsLayer = value;
                foreach (var pushpin in PinPool)
                {
                    pushpin.Template = Application.Current.Resources["MetroStationTemplate"] as ControlTemplate;
                    pushpin.ApplyTemplate();
               //     VisualStateManager.GoToState(pushpin, "Normal", false);
                //    pushpin.ApplyTemplate();

                    // _stationsLayer.Children.Add(pushpin);
                }
            }
        }


        private MapLayer itineraryLayer;
        public MapLayer ItineraryLayer
        {
            get { return itineraryLayer; }
            set
            {
                itineraryLayer = value;
            }
        }


        private AddressViewModel addressViewModel;
        public AddressViewModel AddressViewModel
        {
            get { return addressViewModel ?? (addressViewModel = new AddressViewModel(this)); }
        }
        private GPSViewModel _gPSViewModel;
        public GPSViewModel GPSViewModel
        {
            get { return _gPSViewModel ?? (_gPSViewModel = new GPSViewModel(this)); }
        }
        private TileSourceViewModel tileSourceViewModel;
        public TileSourceViewModel TileSourceViewModel
        {
            get { return tileSourceViewModel ?? (tileSourceViewModel = new TileSourceViewModel(this)); }
        }
        private ItineraryViewModel itineraryViewModel;
        public ItineraryViewModel ItineraryViewModel
        {
            get { return itineraryViewModel ?? (itineraryViewModel = new ItineraryViewModel(this)); }
        }

        /// <summary>
        /// Bool used to notify that the model is getting informations
        /// </summary>
        private const string IsBusyName = "IsBusy";
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                App.BusyMessage.IsOpen = value;
                if (_isBusy != value)
                {
                    _isBusy = value;
                  
                    RaisePropertyChanged(IsBusyName);
                }
            }
        }

        private readonly List<Station> _stations = new List<Station>();
        public  readonly List<Station> DistincStations = new List<Station>();
        public   readonly List<string> StationNames = new List<string>();

        private readonly ObservableCollection<MapItem> _items = new ObservableCollection<MapItem>();
        public ObservableCollection<MapItem> Items
        {
            get
            {
                return _items;
            }
        }

        private readonly ObservableCollection<MapItem> _metroLines = new ObservableCollection<MapItem>();
        public ObservableCollection<MapItem> MetroLines
        {
            get
            {
                return _metroLines;
            }
        }

        private readonly CollectionViewSource _itemsView = new CollectionViewSource();
        public ICollectionView ItemsView
        {
            get
            {
                return _itemsView.View;
            }
        }


        private double stationsLabelOpacity = 1;
        public double StationsLabelOpacity
        {
            get { return stationsLabelOpacity; }
            set
            {
                if (stationsLabelOpacity != value)
                {
                    stationsLabelOpacity = value;
                    StationsLabelHitTestVisible = stationsLabelOpacity != 0;
                    RaisePropertyChanged("StationsLabelOpacity");
                }
            }
        }

        private static SolidColorBrush _blackColor = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush _whiteColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush stationsLabelColor = _whiteColor;
        public SolidColorBrush StationsLabelColor
        {
            get { return stationsLabelColor; }
            set
            {
                if (stationsLabelColor != value)
                {
                    stationsLabelColor = value;
                    RaisePropertyChanged("StationsLabelColor");
                }
            }
        }

        private bool stationsLabelHitTestVisible = true;
        public bool StationsLabelHitTestVisible
        {
            get { return stationsLabelHitTestVisible; }
            set
            {
                if (stationsLabelHitTestVisible != value)
                {
                    stationsLabelHitTestVisible = value;
                    RaisePropertyChanged("StationsLabelHitTestVisible");
                }
            }
        }
        private Visibility stationsLabelVisibility = Visibility.Visible;
        public Visibility StationsLabelVisibility
        {
            get { return stationsLabelVisibility; }
            set
            {
                if (stationsLabelVisibility != value)
                {
                    stationsLabelVisibility = value;
                    RaisePropertyChanged("StationsLabelVisibility");
                }
            }
        }

        private Visibility _tileLayerVisibility = Visibility.Visible;
        public Visibility TileLayerVisibility
        {
            get { return _tileLayerVisibility; }
            set
            {
                if (_tileLayerVisibility != value)
                {
                    _tileLayerVisibility = value;
                    RaisePropertyChanged("TileLayerVisibility");
                }
            }
        }
      private double _tileLayerOpacity = 0.5;
      public double TileLayerOpacity
        {
            get { return _tileLayerOpacity; }
            set
            {
                if (_tileLayerOpacity != value)
                {
                    _tileLayerOpacity = value;
                    RaisePropertyChanged("TileLayerOpacity");
                    TileLayerVisibility = _tileLayerOpacity == 0 ? Visibility.Collapsed : Visibility.Visible;
                    // si le fond de carte est trop clair, changement de couleurs des labels des stations
                    StationsLabelColor = _tileLayerOpacity > 0.7 ? _blackColor : _whiteColor;
                   // StationsLabelOpacity = _tileLayerOpacity > 0.9 ? 0 : 1;
                    StationsLabelVisibility = _tileLayerOpacity > 0.7 ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }
      private string _currentVisualState;
      public string CurrentVisualState
      {
          get { return _currentVisualState; }
          set
          {

              if (value != _currentVisualState)
              {
                  _currentVisualState = value;
                  RaisePropertyChanged("CurrentVisualState");
              }
          }
      }


      private BitmapImage linesImageSource;
      public BitmapImage LinesImageSource
      {
          get { return linesImageSource; }
          set
          {

              if (value != linesImageSource)
              {
                  linesImageSource = value;
                  RaisePropertyChanged("LinesImageSource");
              }
          }
      }


      private Station _selectedStation;
      public Station SelectedStation
      {
          get { return _selectedStation; }
          set
          {

              if (value != _selectedStation)
              {
                  _selectedStation = value;
                  if (_selectedStation!=null)
                    _selectedStation.Selected = true;
                  RaisePropertyChanged("SelectedStation");
              }
          }
      }

      private MapItem _selectedMetroSearch;
      public MapItem SelectedMetroSearch
      {
          get { return _selectedMetroSearch; }
          set
          {

              if (value != _selectedMetroSearch)
              {
                  _selectedMetroSearch = value;
                  RaisePropertyChanged("SelectedMetroSearch");
              }
          }
      }
      private string _selectedMetroSearchText;
        public string SelectedMetroSearchText
      {
          get { return _selectedMetroSearchText; }
          set
          {

              if (value != _selectedMetroSearchText)
              {
                  _selectedMetroSearchText = value;
                  RaisePropertyChanged("SelectedMetroSearchText");
              }
          }
      }
       
      #region commandes


        private RelayCommand backBorderLostFocusCommand;
        public RelayCommand BackBorderLostFocusCommand
        {
            get
            {
                return backBorderLostFocusCommand ?? (backBorderLostFocusCommand = new RelayCommand(() => Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    CurrentVisualState = "NormalState";
                    CurrentVisualState = "HideBackground";
                    Map.Focus();
                })));
            }
        }


      



      private RelayCommand<SelectionChangedEventArgs> _searchCommand;
      public RelayCommand<SelectionChangedEventArgs> SearchCommand
      {
          get
          {
              return _searchCommand ?? (_searchCommand = new RelayCommand<SelectionChangedEventArgs>(eventArgs=>
                        {
                            if (eventArgs.AddedItems.Count <= 0)
                                return;
                            if(eventArgs.AddedItems[0] == null)
                                return;
                            
                            if(eventArgs.AddedItems[0] is Station)
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if(ItineraryViewModel.ItineraryMode)
                                        ItineraryViewModel.Cleanup();

                                    CurrentVisualState = "HideBackground";

                                    var station = eventArgs.AddedItems[0] as Station;
                                    var pin = PinPool.Where(p => p.DataContext == null).FirstOrDefault();
                                    if (pin == null)
                                        pin = PinPool.FirstOrDefault();
                                   
                                    _stationsLayer.Children.Remove(pin);
                                    AddPinToStationLayer(pin, station);

                                    Map.AnimationLevel = AnimationLevel.None; 
                                    Map.SetView((eventArgs.AddedItems[0] as Station).Location, 15);
                                    Map.AnimationLevel = AnimationLevel.Full; 
                                    SelectStation(pin);
                                    SelectedMetroSearchText = string.Empty;

                                    Map.Focus();
                                });
                                return;
   
                            }
                            if (eventArgs.AddedItems[0] is MetroLine)
                            {
                                var metroLine = (eventArgs.AddedItems[0] as MetroLine);
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    TileLayerOpacity = 0.1;
                                    Map.AnimationLevel = AnimationLevel.None; 
                                    Map.SetView(LocationRect.CreateLocationRect(metroLine.Locations));
                                    Map.AnimationLevel = AnimationLevel.Full; 
                                    UnselectLines();
                                    metroLine.Select();
                                    CurrentVisualState = "HideBackground";
                                    CurrentVisualState = "NormalState";
                                    CurrentVisualState = "LineSelectedState";
                                    SelectedMetroSearchText = string.Empty;

                                    Map.Focus();
                                });
                                return;

                            }
                        }));
          }
      }

     

      private void UnselectLines()
      {
          foreach (var metroLine in MetroLines.Where(m=>m.Selected))
          {
              metroLine.UnSelect();
          }
      }

      private RelayCommand _backKeyPressCommand;
        public RelayCommand BackKeyPressCommand
      {
          get
          {
              return _backKeyPressCommand ?? (_backKeyPressCommand = new RelayCommand(() => Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                                    {
                                                                                        if (CurrentVisualState != "NormalState")
                                                                                        {
                                                                                            CurrentVisualState = "HideBackground";
                                                                                            CurrentVisualState = "NormalState";
                                                                                           
                                                                                        }
                                                                                           
                                                                                    })));
          }
      }


      private RelayCommand _showSearchCommand;
      public RelayCommand ShowSearchCommand
      {
          get
          {
              return _showSearchCommand ?? (_showSearchCommand = new RelayCommand(() => Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                                    {
                                                                                        if (CurrentVisualState != "SearchState")
                                                                                        {
                                                                                            CurrentVisualState = "ShowBackground";
                                                                                            CurrentVisualState ="SearchState";
                                                                                        }else
                                                                                        {
                                                                                            
                                                                                            CurrentVisualState = "HideBackground";
                                                                                            CurrentVisualState = "NormalState";
                                                                                            Map.Focus();
                                                                                        }
                                                                                           
                                                                                    })));
          }
      }
#endregion
    

      void pin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
      {
          // unselect others pin before select one
          UnselectStations();
          SelectStation(sender as Pushpin);
      }
  

      private void SelectStation(Control pin)
      {
          UnselectStations();
          VisualStateManager.GoToState(pin, "Selected", false);
          CurrentVisualState = "SelectedState";
          SelectedStation = pin.DataContext as Station;
          pin.SetValue(Canvas.ZIndexProperty, 1000);
      }

     
      private void AddSelectStation(Control pin)
      {
          VisualStateManager.GoToState(pin, "Selected", false);
          CurrentVisualState = "SelectedState";
          SelectedStation = pin.DataContext as Station;
      }


      internal void FocusStation(Control pin)
      {
          var station = pin.DataContext as Station;
          Map.SetView(station.Location, 15);
          VisualStateManager.GoToState(pin, "Selected", false);
      }

      internal void UnselectStations()
        {
            if (AddressViewModel.POILookUpSelected)
            {
                AddressViewModel.POILookUpSelected = false;
                VisualStateManager.GoToState(AddressViewModel.POILookup, "Normal", false);
            }
          for (int i = 0; i < PinPool.Count;i++ )
            {
                if (PinPool[i].DataContext != null)
                {
                    VisualStateManager.GoToState(PinPool[i], "Normal", false);
                    PinPool[i].SetValue(Canvas.ZIndexProperty, 800);
                }
            }
         if(SelectedStation!=null)
         {
             SelectedStation.Selected = false;
             SelectedStation = null;
         }
         
        }

        internal void CleanUp()
        {
            UnselectStations();
            SelectedStation = null;
            AddressViewModel.POILookUpSelected = false;
            VisualStateManager.GoToState(AddressViewModel.POILookup, "Normal", false);
            CurrentVisualState = "NormalState";
        }

      


      //private string _currentText;
      //public bool MetroFilter(string search, object value)
      //{
      //    if (value != null)
      //    {
      //        if (_currentText != search.RemoveAccents())
      //            _currentText = search.RemoveAccents();
      //        return (value as MapItem).NormalizedName.Contains(_currentText);
      //    }
      //    return false;
      //} 

        private void LoadKML()
        {
            var xElement = XElement.Load(Application.GetResourceStream(new Uri("/METRO;component/Assets/KML/Metro_rer_Paris_v3_0.kml", UriKind.RelativeOrAbsolute)).Stream);
            LoadLignesMetro(xElement);
            LoadMetroStations(xElement);
            _itemsView.Source = Items.Concat(MetroLines);

        }
        private void LoadMetroStations(XElement xElement)
        {
            var lignesMetro = xElement.Descendants(ns + "Folder")
            .Where(f => f.Descendants(ns + "name")
               .First().Value.Equals("LIGNES METRO", StringComparison.InvariantCultureIgnoreCase))
               .Descendants(ns + "Folder");

            var traitsLignesMetro = xElement.Descendants(ns + "Folder")
            .Where(f => f.Descendants(ns + "name")
            .First().Value.Equals("Traits lignes metro", StringComparison.InvariantCultureIgnoreCase))
            .Descendants(ns + "Placemark");

          
            var stationCounter = 0;
            foreach (var ligneMetro in lignesMetro)
            {
                
                var metroNumber = ligneMetro.Descendants(ns + "name").First().Value.Replace("Ligne ", string.Empty);

                var placemark = traitsLignesMetro.Where(p => p.Descendants(ns + "name").First().Value == metroNumber).First();
                var styleURL = placemark.Descendants(ns + "styleUrl").First().Value.Replace("#", string.Empty);
                var colorString = xElement.Descendants(ns + "Style").Where(s => s.Attribute("id").Value == styleURL).First().Descendants(ns + "color").First().Value;
                var color = GetColorFromString(colorString);
                var metroStations = ligneMetro.Descendants(ns + "Placemark");

                Station previousStation=null;
                var topStation = metroStations.FirstOrDefault().Descendants(ns + "name").FirstOrDefault().Value;
                topStation = topStation.Remove(topStation.IndexOf('('), topStation.Length - topStation.IndexOf('(')).Trim();
                var bottomStation = metroStations.Last().Descendants(ns + "name").FirstOrDefault().Value;
                bottomStation = bottomStation.Remove(bottomStation.IndexOf('('), bottomStation.Length - bottomStation.IndexOf('(')).Trim();
                foreach (var metroStation in metroStations)
                {
                    var name = metroStation.Descendants(ns + "name").FirstOrDefault().Value;

                    name = name.Remove(name.IndexOf('('), name.Length - name.IndexOf('(')).Trim();
                   // var longitude = metroStation.Descendants(ns + "longitude").FirstOrDefault().Value;
                   // var latitude = metroStation.Descendants(ns + "latitude").FirstOrDefault().Value;
                    var coordinatesString = metroStation.Descendants(ns + "coordinates").FirstOrDefault().Value;
                    var coordinatesArray = RemoveUnwantedChars(coordinatesString).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var latitude = double.Parse(coordinatesArray[1], CultureInfo.InvariantCulture);
                    var longitude = double.Parse(coordinatesArray[0], CultureInfo.InvariantCulture);
                   
                    var station = new Station
                    {
                        Id = stationCounter,
                        Name =  name,
                        NormalizedName = name.RemoveAccents(),
                        Location = new GeoCoordinate( latitude, longitude),
                        Color = new SolidColorBrush(color),
                        TopStation = topStation,
                        BottomStation = bottomStation,
                        PreviousStation = previousStation,
                        CurrentLine = metroNumber
                    };
                    station.MetroLignes.Add(metroNumber);
                    _stations.Add(station);

                    if(!StationNames.Contains(station.NormalizedName))
                    {
                        StationNames.Add(station.NormalizedName);
                        DistincStations.Add(station);
                    }
                   
                    stationCounter++;


                    // Ajout de l'arc
                    if (previousStation != null)
                    {
                        var distance = (int)previousStation.Location.GetDistanceTo(station.Location);
                        var cost = distance/10;
                        ShortestPath.Edges.Add(new Edge
                        {
                            Station1 = previousStation,
                            Station2 = station,
                            Distance = distance,
                            Cost = cost
                        });
                        ShortestPath.Edges.Add(new Edge
                        {
                            Station1 = station,
                            Station2 = previousStation,
                            Distance = distance,
                            Cost = cost
                        });
                    }
                    
                    previousStation = station;
               

                    // ajout de la ligne à la station
                    if (Items.Contains(station)){
              
                        var alreadyExistStation = Items.Where(s => s.Name == station.Name).FirstOrDefault() as Station;
                        alreadyExistStation.MetroLignes.Add(metroNumber);

                        // Ajout des arc pour les autres lignes
                        foreach (var sameNameStation in _stations.Where(s => s.Name == station.Name))
                        {
                            ShortestPath.Edges.Add(new Edge
                            {
                                Station1 = station,
                                Station2 = sameNameStation,
                                Distance = 150,
                                Cost = 300
                            });
                            ShortestPath.Edges.Add(new Edge
                            {
                                Station1 = sameNameStation,
                                Station2 = station,
                                Distance = 150,
                                Cost = 300
                            });
                        }
                      
                        // On ne la dupplique pas pour la carte
                        continue;
                    }
                    Items.Add(station);
            
                }
            }

            ShortestPath.InitMatrix(_stations);

           
           

        }


        private void LoadLignesMetro(XElement xElement)
        {
            var lignesMetro = xElement.Descendants(ns + "Folder")
           .Where(f => f.Descendants(ns + "name")
               .First().Value.Equals("Traits lignes metro", StringComparison.InvariantCultureIgnoreCase))
               .Descendants(ns + "Placemark");

            string _previsousColor = null;
            SolidColorBrush color = null;

            foreach (var ligneMetro in lignesMetro)
            {
                var name = ligneMetro.Descendants(ns + "name").First().Value;
                name = "Ligne " + name;
                var coordinatesString = ligneMetro.Descendants(ns + "coordinates").First().Value;
                var coordinatesArray = RemoveUnwantedChars(coordinatesString).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var styleURL = ligneMetro.Descendants(ns + "styleUrl").First().Value.Replace("#", string.Empty);
                var colorString = xElement.Descendants(ns + "Style").Where(s => s.Attribute("id").Value == styleURL).First().Descendants(ns + "color").First().Value;

                var locations = new LocationCollection();
                foreach (var coordinateStr in coordinatesArray)
                {
                    var coordinate = coordinateStr.Split(',');
                    var c = double.Parse(coordinate[1], CultureInfo.InvariantCulture);
                    var c2 = double.Parse(coordinate[0], CultureInfo.InvariantCulture);
                    locations.Add(new GeoCoordinate(c, c2));
                }
                var metroLine = new MetroLine();
                metroLine.Locations = locations;
                metroLine.Name = name;
                metroLine.NormalizedName = name.RemoveAccents();
               
                if (_previsousColor !=  colorString)
                {
                    _previsousColor = colorString;
                    color = new SolidColorBrush(GetColorFromString(colorString));
                }
                metroLine.Color = color;
                MetroLines.Add(metroLine);
            }
        }

        /// <param name="color">format : AABBVVRR</param>
        /// <returns></returns>
        private static Color GetColorFromString(string color)
        {
            byte a = 255;
            var r = (byte)(Convert.ToUInt32(color.Substring(6, 2), 16));
            var g = (byte)(Convert.ToUInt32(color.Substring(4, 2), 16));
            var b = (byte)(Convert.ToUInt32(color.Substring(2, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }

        private static string[] _unWantedChars = new string[] { "\r", "\n", "\t" };
        private static string RemoveUnwantedChars(string value)
        {
            foreach (string s in _unWantedChars)
            {
                value = value.Replace(s, string.Empty);
            }
            return value;
        }
    }
}