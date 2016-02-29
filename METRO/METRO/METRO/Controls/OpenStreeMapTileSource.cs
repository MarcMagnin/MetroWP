using System;
using Microsoft.Phone.Controls.Maps;

namespace METRO.Controls
{
    public class OpenStreetMapTileSource : TileSource
    {
        public OpenStreetMapTileSource()
            : base("http://tile.openstreetmap.org/{2}/{0}/{1}.png"){}

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            return new Uri(string.Format(UriFormat, x, y, zoomLevel));
        }
    }
}
