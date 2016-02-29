/*
 * Author: Jeppe Andersen
 * Website: nocture.dk
 * 
 * Feel free to use this in any way you want :-)
 * 
 */
using System;
using System.Drawing;

namespace Dijkstra
{
    public class Vertex
    {
        public Point p { get; private set; }
        public int ident {get; private set; }
        public int dist { get; set; }

        public Vertex(Point p, int ident)
        {
            this.p = p;
            this.ident = ident;
        }
    }
}
