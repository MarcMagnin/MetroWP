using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Linq;

namespace METRO.Model
{
    public class Station : MapItem
    {
        private static readonly Dictionary<string, BitmapImage> StationImages = new Dictionary<string, BitmapImage>
        {
              {"1",new BitmapImage(new Uri("../Assets/Images/M1.png",UriKind.Relative))},                                                          
              {"2",new BitmapImage(new Uri("../Assets/Images/M2.png",UriKind.Relative))},
              {"3",new BitmapImage(new Uri("../Assets/Images/M3.png",UriKind.Relative))},
              {"3bis",new BitmapImage(new Uri("../Assets/Images/M3B.png",UriKind.Relative))},
              {"4",new BitmapImage(new Uri("../Assets/Images/M4.png",UriKind.Relative))},
              {"5",new BitmapImage(new Uri("../Assets/Images/M5.png",UriKind.Relative))},
              {"6",new BitmapImage(new Uri("../Assets/Images/M6.png",UriKind.Relative))},
              {"7",new BitmapImage(new Uri("../Assets/Images/M7.png",UriKind.Relative))},
              {"7bis",new BitmapImage(new Uri("../Assets/Images/M7B.png",UriKind.Relative))},
              {"8",new BitmapImage(new Uri("../Assets/Images/M8.png",UriKind.Relative))},
              {"9",new BitmapImage(new Uri("../Assets/Images/M9.png",UriKind.Relative))},
              {"10",new BitmapImage(new Uri("../Assets/Images/M10.png",UriKind.Relative))},
              {"11",new BitmapImage(new Uri("../Assets/Images/M11.png",UriKind.Relative))},
              {"12",new BitmapImage(new Uri("../Assets/Images/M12.png",UriKind.Relative))},
              {"13",new BitmapImage(new Uri("../Assets/Images/M13.png",UriKind.Relative))},
              {"14",new BitmapImage(new Uri("../Assets/Images/M14.png",UriKind.Relative))},
        };


        private List<string> metroLignes;
        public List<string> MetroLignes
        {
            get { return metroLignes ?? (metroLignes = new List<string>()); }
        }

        private List<string> rerLignes;
        public List<string> RerLignes
        {
            get { return rerLignes ?? (rerLignes = new List<string>()); }
        }

        private List<string> tramLignes;
        public List<string> TramLignes
        {
            get { return tramLignes ?? (tramLignes = new List<string>()); }
        }

        public bool Added { get; set; }
        public GeoCoordinate Location { get; set; }
        public string TopStation { get; set; }
        public string BottomStation { get; set; }
        public Station PreviousStation { get; set; }
        public string CurrentLine { get; set; }

        public BitmapImage CurrentLineImage
        {
            get { return StationImages[CurrentLine]; }
        }

        private string distance;
        public string Distance
        {
            get { return distance; }
            set
            {
                if (distance != value)
                {
                    distance = value;
                    RaisePropertyChanged("Distance");
                }
            }
        }
   

        private Visibility _visibility = Visibility.Collapsed;
        public Visibility Visibility { get { return _visibility; } set
        {
            if(_visibility != value)
            {
                _visibility = value;
                RaisePropertyChanged("Visibility");
            }
        } }

        public override string ToString()
        {
            return Name;
        }

        public string FullName
        {
            get { return Name + MetroLignes.Aggregate(" ( ", (current, next) => current + "M" + next + " ") + ")"; }
            
        }


        public string StationsConcatened
        {
            get { return MetroLignes.Aggregate(" ( ", (current, next) => current + "M" + next + " ") + ")"; }

        }

        public List<BitmapImage> StationsImage
        {
            get
            {
                return StationImages.Where(t => MetroLignes.Contains(t.Key)).Select(t => t.Value).ToList();
            }
        }
       

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Name.Equals(((Station)obj).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

    }
}
