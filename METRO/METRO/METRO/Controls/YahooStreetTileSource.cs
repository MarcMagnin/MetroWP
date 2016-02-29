using System;
using Microsoft.Phone.Controls.Maps;

namespace METRO.Controls
{
    public class YahooStreetTileSource : TileSource
    {
        public YahooStreetTileSource()
            : base("http://us.maps2.yimg.com/us.png.maps.yimg.com/png?v=3.52&t=m&x={0}&y={1}&z={2}")
        {
        }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            // The math used here was copied from the DeepEarth Project (http://deepearth.codeplex.com) 
            double posY;
            double zoom = 18 - zoomLevel;
            double num4 = Math.Pow(2.0, zoomLevel) / 2.0;

            if (y < num4)
            {
                posY = (num4 - Convert.ToDouble(y)) - 1.0;
            }
            else
            {
                posY = ((Convert.ToDouble(y) + 1) - num4) * -1.0;
            }

            return new Uri(String.Format(UriFormat, x, posY, zoom));
        }
    }
}
