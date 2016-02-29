using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using METRO.Model;
using System.Linq;
using METRO.ViewModel;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;

namespace METRO
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            InitStoryBoardHandlers();
            //MetroAutoComplete.ItemFilter += (DataContext as MainViewModel).MetroFilter;
            (DataContext as MainViewModel).Map = MainMap;
            (DataContext as MainViewModel).StationsLayer= StationsLayer;
        
            (DataContext as MainViewModel).MetroAutoComplete = MetroAutoComplete;
            (DataContext as MainViewModel).TileSourceViewModel.TileLayer = TileLayer;

            (DataContext as MainViewModel).ItineraryViewModel.FromAutoComplete = FromAutoComplete;
            (DataContext as MainViewModel).ItineraryViewModel.ToAutoComplete = ToAutoComplete;

            (DataContext as MainViewModel).AddressViewModel.POILookup= POILookup;

            FromAutoComplete.KeyUp += TextKeyUp;
            ToAutoComplete.KeyUp += TextKeyUp;
            MetroAutoComplete.KeyUp += TextKeyUp;



            // Permet de retirer le message d'erreur de la carte bing quand l'utilisateur n'a pas de réseau
            MainMap.LoadingError += (s, e) =>
                                        {
                                            e.Handled = true;
                                            var layer = (MapLayer)MainMap.Content;
                                            // loading error message is last layer in list
                                            var myType = layer.Children.ElementAt(layer.Children.Count - 1).ToString();
                                            try
                                            {
                                                layer.Children.RemoveAt(layer.Children.Count - 1);
                                            }
                                            catch
                                            {
                                                // print out the type in case something goes wrong
                                                MessageBox.Show("Loading Error", myType, MessageBoxButton.OK);
                                            }
                                        };
        }

        void TextKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                (DataContext as MainViewModel).BackKeyPressCommand.Execute(null);
            } 
        }


        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            // annulation des résolution d'adresse 
            if ((DataContext as MainViewModel).AddressViewModel.GeoCodeService != null)
            {

                (DataContext as MainViewModel).AddressViewModel.Cleanup();

                e.Cancel = true;
            }
            if ((DataContext as MainViewModel).ItineraryViewModel.GeoWrapper != null)
            {
                (DataContext as MainViewModel).ItineraryViewModel.Cleanup();
                e.Cancel = true;
            }

            if ((DataContext as MainViewModel).ItineraryViewModel.ItineraryMode)
            {
                (DataContext as MainViewModel).ItineraryViewModel.Cleanup();
                e.Cancel = true;
            }
            if ((DataContext as MainViewModel).SelectedStation != null)
            {
                (DataContext as MainViewModel).CleanUp();
                e.Cancel = true;
            }

            if ((DataContext as MainViewModel).AddressViewModel.POILookUpSelected)
            {
               
                (DataContext as MainViewModel).CleanUp();
               
                e.Cancel = true;
            }

            
        }

        
        private void InitStoryBoardHandlers()
        {
            // show search menu storyboard
            ((VisualState)VisualStateGroup.States[1]).Storyboard.Completed += (s, e) =>
            {

                layoutSearch.CacheMode = null;
                ThreadPool.QueueUserWorkItem(f =>
                        {
                           // Thread.Sleep(80);
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        MetroAutoComplete.Focus();
                                    });

                        });

            };

            // show itinerary menu storyboard
            ((VisualState)VisualStateGroup.States[3]).Storyboard.Completed += (s, e) =>
            {

                layoutItinerary.CacheMode = null;
                ThreadPool.QueueUserWorkItem(f =>
                {
                    Thread.Sleep(80);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        FromAutoComplete.Focus();
                    });

                });

            };
        }

        
        private void MapPolyline_Loaded(object sender, RoutedEventArgs e)    
        {
            var polygon = sender as MapPolyline;
            var model = polygon.DataContext as MetroLine;
            polygon.Stroke = model.Color;
            (DataContext as MainViewModel).MetroLinesControl.Add(polygon);
        }

        private void ApplicationBarSearchButton_Click(object sender, EventArgs e)
        {
            layoutSearch.CacheMode = new BitmapCache();
            (DataContext as MainViewModel).UnselectStations();
        	(DataContext as MainViewModel).ShowSearchCommand.Execute(null);
        }
        private void GoToMyLocationClick(object sender, EventArgs e)
        {
            (DataContext as MainViewModel).UnselectStations();
            (DataContext as MainViewModel).GPSViewModel.ShowMyLocationCommand.Execute(sender);
          
        }
        private void ApplicationBarChangeTileSource_Click(object sender, EventArgs e)
        {
            layoutItinerary.CacheMode = new BitmapCache();
            (DataContext as MainViewModel).CurrentVisualState = "StopChangeTileSource";
            (DataContext as MainViewModel).TileSourceViewModel.ChangeTileSourceCommand.Execute(sender);
        }

        private void ItineraryClick(object sender, EventArgs e)
        {
            (DataContext as MainViewModel).UnselectStations();
            (DataContext as MainViewModel).ItineraryViewModel.ShowHideItineraryCommand.Execute(sender);
        }

        private void Credits_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Credits.xaml", UriKind.Relative));
        }
    }
}