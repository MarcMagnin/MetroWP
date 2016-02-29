using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DataStructTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int totalNodes=100000;
        private float[,] cost;
        private DijkstraFast dijkstra;
        private Random rv;
        private void Form1_Load(object sender, EventArgs e)
        {
            cost = new float[totalNodes, 4];
            // initialize the cost matrix
            rv= new Random();
            for (int i = 0; i < totalNodes; i++)
            {
                cost[i, 0] = (float)rv.Next(1000)*0.01f;
                cost[i, 1] = (float)rv.Next(1000) * 0.01f;
                cost[i, 2] = (float)rv.Next(1000) * 0.01f;
                cost[i, 3] = (float)rv.Next(1000) * 0.01f;
            }

            dijkstra = new DijkstraFast(totalNodes,
                                            new DijkstraFast.InternodeTraversalCost(getInternodeTraversalCost),
                                            new DijkstraFast.NearbyNodesHint(GetNearbyNodes));
        }

        
        // a function to get relative position from one node to another
        private int GetRelativePosition(int start, int finish)
        {
            if (start - 1 == finish)
                return 0;

            else if (start +1 == finish)
                return 1;

            else if (start +5  == finish)
                return 2;

            else if (start -5 == finish)
                return 3;

            return -1;
        }

        // get costs. If there is no connection, then cost is maximum.
        private float getInternodeTraversalCost(int start, int finish)
        {
            int relativePosition = GetRelativePosition(start, finish);
            if (relativePosition < 0) return float.MaxValue;
            return cost[start, relativePosition];
        }

        private IEnumerable<int> GetNearbyNodes(int startingNode)
        {
            List<int> nearbyNodes = new List<int>(4);

            if (startingNode >= totalNodes-5) startingNode = -1;
            if (startingNode <=5) startingNode = -1;

            // in the order as defined in GetRelativePosition
            nearbyNodes.Add(startingNode-1);
            nearbyNodes.Add(startingNode+1);
            nearbyNodes.Add(startingNode+5);
            nearbyNodes.Add(startingNode-5);
            return nearbyNodes;
        }

        // compute
        private void button1_Click(object sender, EventArgs e)
        {
             int t1 = Environment.TickCount;

             int[] minPath = dijkstra.GetMinimumPath(int.Parse(textBox2.Text), int.Parse(textBox3.Text));

            int t2 = Environment.TickCount;

            listBox1.Items.Clear();
            for (int i = 0; i != minPath.Length; i++)
                listBox1.Items.Add(minPath[i]);
            int diff = (t2 - t1);
            MessageBox.Show("Path finding took "+diff.ToString()+" ms, Minimum item in list box: "+ minPath[minPath.Length-1].ToString());
        }

        // initialize
        private void button2_Click(object sender, EventArgs e)
        {
            totalNodes=int.Parse(textBox1.Text);
            cost = new float[totalNodes, 4];
            // initialize the cost matrix
            rv = new Random();
            for (int i = 0; i < totalNodes; i++)
            {
                cost[i, 0] = (float)rv.Next(1000) * 0.01f;
                cost[i, 1] = (float)rv.Next(1000) * 0.01f;
                cost[i, 2] = (float)rv.Next(1000) * 0.01f;
                cost[i, 3] = (float)rv.Next(1000) * 0.01f;
            }

            dijkstra = new DijkstraFast(totalNodes,
                                            new DijkstraFast.InternodeTraversalCost(getInternodeTraversalCost),
                                            new DijkstraFast.NearbyNodesHint(GetNearbyNodes));
        }

    }
}