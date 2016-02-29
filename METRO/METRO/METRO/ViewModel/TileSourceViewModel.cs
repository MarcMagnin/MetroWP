using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using METRO.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Core;

namespace METRO.ViewModel
{
    public class TileSourceViewModel : ViewModelBase
    {

        private MainViewModel mainViewModel;

        private readonly LinkedList<TileSourceBox> _tileSources = new LinkedList<TileSourceBox>();
        private LinkedListNode<TileSourceBox> _currentTileNode;
        public LinkedListNode<TileSourceBox> CurrentTileNode
        {
            get { return _currentTileNode; }
            set { _currentTileNode = value;
            RaisePropertyChanged("CurrentTileNode");
            }
        }

        private RelayCommand _changeTileSourceCommand;
        public RelayCommand ChangeTileSourceCommand
        {
            get
            {
                return _changeTileSourceCommand ?? (_changeTileSourceCommand = new RelayCommand(() =>
                {
                   
                    TileLayer.TileSources.Clear();
                    CurrentTileNode = _currentTileNode.Next ?? _currentTileNode.List.First;
                    TileLayer.TileSources.Add(_currentTileNode.Value.TileSource);
                    mainViewModel.CurrentVisualState = "ChangeTileSource";
                }));
            }
        }

        public MapTileLayer TileLayer { get; set; }

        public TileSourceViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            _tileSources.AddLast(new TileSourceBox{ Name = "Open Street Map", TileSource = new OpenStreetMapTileSource()});
            _tileSources.AddLast(new TileSourceBox { Name = "Bing Map", TileSource = new BingMapTileSource() });
            _tileSources.AddLast(new TileSourceBox { Name = "Google Map", TileSource = new GoogleMapsRoadTileSource() });
            _tileSources.AddLast(new TileSourceBox { Name = "Yahoo Street", TileSource = new YahooStreetTileSource() });

            _currentTileNode = _tileSources.First;
        }

        public class TileSourceBox
        {
            public TileSource TileSource { get; set; }
            public string Name { get; set; }
        }
    }
}
