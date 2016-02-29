using System.Device.Location;
using Microsoft.Phone.Controls.Maps;

namespace METRO.Helpers
{
    public static class Extensions
    {
        public static bool Contains(this LocationRect r, GeoCoordinate p)
        {
            double w = r.West - 0.003;
            double e = r.East + 0.001;
            double s = r.South - 0.001;
            double n = r.North + 0.001;
            return w <= p.Longitude && p.Longitude <= e && s <= p.Latitude && p.Latitude <= n;
        }

     
        public static string RemoveAccents(this string s)
        {
            var returnValue = s.ToUpper();
            returnValue = returnValue.Replace("À", "A");
            returnValue = returnValue.Replace("Á", "A");
            returnValue = returnValue.Replace("Â", "A");
            returnValue = returnValue.Replace("Ã", "A");
            returnValue = returnValue.Replace("Ä", "A");
            returnValue = returnValue.Replace("Å", "A");
            returnValue = returnValue.Replace("Æ", "A");

            returnValue = returnValue.Replace("Ç", "C");

            returnValue = returnValue.Replace("È", "E");
            returnValue = returnValue.Replace("É", "E");
            returnValue = returnValue.Replace("Ê", "E");
            returnValue = returnValue.Replace("Ë", "E");

            returnValue = returnValue.Replace("Ì", "I");
            returnValue = returnValue.Replace("Í", "I");
            returnValue = returnValue.Replace("Î", "I");
            returnValue = returnValue.Replace("Ï", "I");

            returnValue = returnValue.Replace("Ñ", "N");

            returnValue = returnValue.Replace("Ò", "O");
            returnValue = returnValue.Replace("Ó", "O");
            returnValue = returnValue.Replace("Ô", "O");
            returnValue = returnValue.Replace("Õ", "O");
            returnValue = returnValue.Replace("Ö", "O");

            returnValue = returnValue.Replace("Ù", "U");
            returnValue = returnValue.Replace("Ú", "U");
            returnValue = returnValue.Replace("Û", "U");
            returnValue = returnValue.Replace("Ü", "U");

            returnValue = returnValue.Replace("Ý", "Y");

            returnValue = returnValue.Replace("Æ", "AE");
            returnValue = returnValue.Replace("Œ", "OE");
            return returnValue;
        }

        
    }
}
    