using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using METRO.Helpers;

namespace METRO.Model
{
    public static class ShortestPath
    {
        public static List<Edge> Edges = new List<Edge>();
        public static List<Station> Stations;
        public static List<Station> StationNames;
        public static double[,] G;
        public static void InitMatrix(List<Station> stations)
        {
            Stations = stations;
 
            /* Create the array containing the adjacency matrix */
            G = new double[stations.Count, stations.Count];

            /* Set the connections and weights based on each edge in the collection */
            foreach (var edge in Edges)
            {
                G[edge.Station1.Id, edge.Station2.Id] = edge.Cost;
            }

           
        }

         

        public static int[] GetMinimumPath(int start, int finish, int[] shortestPath)
        {
            var path = new Stack<int>();
            do
            {
                path.Push(finish);
                finish = shortestPath[finish]; // step back one step toward the start point 
            } while (finish != start && finish!=-1);
            return path.ToArray();

        }


        public static string GetMinimumPathStr(int start, int finish, int[] shortestPath)
        {
            var stringBuilder = new StringBuilder();

            var path = new Stack<int>();
            do
            {
                path.Push(finish);
                finish = shortestPath[finish]; // step back one step toward the start point 
            } while (finish != start);
            path.Push(start);
            foreach (var i in path)
            {
                stringBuilder.Append(Stations[i].Name).Append("->");
            }

            return stringBuilder.ToString();

        }
    }
}
