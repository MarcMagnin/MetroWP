/*
 * Author: Jeppe Andersen
 * Website: nocture.dk
 * 
 * Feel free to use this in any way you want :-)
 * 
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Dijkstra
{
    public partial class GUI : Form
    {
        /* Lists of all nodes and edges */
        private List<Vertex> Vertices = new List<Vertex>();
        private List<Edge> Edges = new List<Edge>();

        /* Counter for giving nodes a unique ID */
        private int numV = 0;

        public GUI()
        {
            InitializeComponent();
        }

        /* Adds the vertex that was last created to the comboboxes */
        private void ReloadVertexList()
        {
            cmbFrom.Items.Add(Vertices[Vertices.Count - 1].ident);
            cmbTo.Items.Add(Vertices[Vertices.Count - 1].ident);
        }
        
        /* Draws a vertex on the map */
        private void DrawVertex(int vertex)
        {
            Graphics g = panMap.CreateGraphics();
            
            /* Draws the new vertice */
            Vertex p = Vertices[vertex];
            g.FillEllipse(new SolidBrush(Color.Black), p.p.X - 25, p.p.Y - 25, 50, 50);
            g.DrawString(p.ident.ToString(), new Font("Verdana", 11), new SolidBrush(Color.Black), p.p.X - 40, p.p.Y - 30);
            
            g.Dispose();
        }

        /* Draws an edge on the map */
        private void DrawEdge(int edge)
        {
            Graphics g = panMap.CreateGraphics();
            
            /* Gets the edge to draw and the points it connects */
            Edge p = Edges[edge];
            Vertex from = Vertices[p.from];
            Vertex to = Vertices[p.to];
            Point pFrom;
            Point pTo;

            /* Figures out which side of the nodes to draw the edge */
            pFrom = new Point(from.p.X + (from.p.X < to.p.X ? 1 : -1)* 25, from.p.Y);
            pTo = new Point(to.p.X + (from.p.X < to.p.X ? -1 : 1)*25, to.p.Y);
  

            Pen pen = new Pen(Color.Black, 1);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.ArrowAnchor;

            /* Draws weight and the connecting line */
            g.DrawString(p.weight.ToString(), new Font("Verdana", 11), new SolidBrush(Color.Black), new Point((pFrom.X + pTo.X) / 2, (pFrom.Y + pTo.Y) / 2 + (pFrom.Y < pTo.Y? (-20) : 10)));
            g.DrawLine(pen, pFrom, pTo);

            g.Dispose();
        }


        private void GUI_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        /* Whenever a click happens on the panel, create a new node in the collection and draw it */
        private void panMap_MouseClick(object sender, MouseEventArgs e)
        {
            Vertices.Add(new Vertex(new Point(e.X, e.Y), numV));
            numV++;

            /* Draws the new vertex */
            DrawVertex(Vertices.Count-1);

            /* Update the vertex list */
            ReloadVertexList();
        }

        /* Adds a new Edge */
        private void btnAddEdge_Click(object sender, EventArgs e)
        {
            try
            {
                int from = Int32.Parse(cmbFrom.SelectedItem.ToString());
                int to = Int32.Parse(cmbTo.SelectedItem.ToString());
                Double weight = Double.Parse(txtWeight.Text);

                /* Add the new edge to the collection */
                Edges.Add(new Edge(from, to, weight));

                /* Draws the new edge */
                DrawEdge(Edges.Count - 1);
            }
            catch (FormatException fe)
            {
                MessageBox.Show("No weight specified or wrong format");
            }
            catch (NullReferenceException ne)
            {
                MessageBox.Show("No vertex selected");
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            /* Create the array containing the adjacency matrix */
            double[,] G = new double[Vertices.Count, Vertices.Count];

            /* Set the connections and weights based on each edge in the collection */
            foreach (Edge edge in Edges)
            {
                G[edge.from, edge.to] = edge.weight;
            }

            /* Runs dijkstra */
            try
            {
                Dijkstra dijk = new Dijkstra(G, 0);
                double[] dist = dijk.dist;
                int[] path = dijk.path;

                /* Prints the shortest distances on the nodes */
                for (int i = 0; i < dist.Length; i++)
                {
                    Graphics g = panMap.CreateGraphics();

                    Vertex p = Vertices[i];
                    StringFormat distFormat = new StringFormat();
                    distFormat.Alignment = StringAlignment.Center;
                    g.DrawString(dist[i].ToString(), new Font("Verdana", 11), new SolidBrush(Color.White), p.p.X, p.p.Y - 7, distFormat);

                }
            }
            catch (ArgumentException err)
            {
                MessageBox.Show(err.Message);
            }
        }

        /* Resets everything */
        private void btnReset_Click(object sender, EventArgs e)
        {
            /* Clear the map panel */
            Graphics g = panMap.CreateGraphics();
            g.Clear(DefaultBackColor);

            /* Removes all nodes in Vertices */
            Vertices.RemoveAll(delegate(Vertex v)
            {
                return true;
            });

            /* Removes all edges in Edges */
            Edges.RemoveAll(delegate(Edge edge) {
                return true;
            });

            /* Resets controls */
            cmbFrom.Items.Clear();
            cmbFrom.ResetText();
            cmbTo.Items.Clear();
            cmbTo.ResetText();
            txtWeight.ResetText();

            /* Resets vertex counter */
            numV = 0;

        }
    }
}
