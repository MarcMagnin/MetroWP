//by Tolga Birdal

using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace DataStructTest
{
    /// <summary> 
    /// Implements a generalized Dijkstra's algorithm to calculate 
    /// both minimum distance and minimum path. 
    /// </summary> 
    /// <remarks> 
    /// For this algorithm, all nodes should be provided, and handled 
    /// in the delegate methods, including the start and finish nodes. 
    /// </remarks> 
    public class DijkstraFast
    {
        /// <summary> 
        /// An optional delegate that can help optimize the algorithm 
        /// by showing it a subset of nodes to consider. Very useful 
        /// for limited connectivity graphs. (like pixels on a screen!) 
        /// </summary> 
        /// <param name="startingNode"> 
        /// The node that is being traveled away FROM. 
        /// </param> 
        /// <returns> 
        /// An array of nodes that might be reached from the  
        /// <paramref name="startingNode"/>. 
        /// </returns> 
        public delegate IEnumerable<int> NearbyNodesHint(int startingNode);
        /// <summary> 
        /// Determines the cost of moving from a given node to another given node. 
        /// </summary> 
        /// <param name="start"> 
        /// The node being moved away from. 
        /// </param> 
        /// <param name="finish"> 
        /// The node that may be moved to. 
        /// </param> 
        /// <returns> 
        /// The cost of the transition from <paramref name="start"/> to 
        /// <paramref name="finish"/>, or <see cref="Int32.MaxValue"/> 
        /// if the transition is impossible (i.e. there is no edge between  
        /// the two nodes). 
        /// </returns> 
        public delegate float InternodeTraversalCost(int start, int finish);

        /// <summary> 
        /// Creates an instance of the <see cref="Dijkstra"/> class. 
        /// </summary> 
        /// <param name="totalNodeCount"> 
        /// The total number of nodes in the graph. 
        /// </param> 
        /// <param name="traversalCost"> 
        /// The delegate that can provide the cost of a transition between 
        /// any two nodes. 
        /// </param> 
        /// <param name="hint"> 
        /// An optional delegate that can provide a small subset of nodes 
        /// that a given node may be connected to. 
        /// </param> 
        public DijkstraFast(int totalNodeCount, InternodeTraversalCost traversalCost, NearbyNodesHint hint)
        {
            if (totalNodeCount < 3) throw new ArgumentOutOfRangeException("totalNodeCount", totalNodeCount, "Expected a minimum of 3.");
            if (traversalCost == null) throw new ArgumentNullException("traversalCost");
            Hint = hint;
            TraversalCost = traversalCost;
            TotalNodeCount = totalNodeCount;
        }

        protected readonly NearbyNodesHint Hint;
        protected readonly InternodeTraversalCost TraversalCost;
        protected readonly int TotalNodeCount;

        public struct Results
        {
            /// <summary> 
            /// Prepares a Dijkstra results package. 
            /// </summary> 
            /// <param name="minimumPath"> 
            /// The minimum path array, where each array element index corresponds  
            /// to a node designation, and the array element value is a pointer to 
            /// the node that should be used to travel to this one. 
            /// </param> 
            /// <param name="minimumDistance"> 
            /// The minimum distance from the starting node to the given node. 
            /// </param> 
            public Results(int[] minimumPath, float[] minimumDistance)
            {
                MinimumDistance = minimumDistance;
                MinimumPath = minimumPath;
            }

            /// The minimum path array, where each array element index corresponds  
            /// to a node designation, and the array element value is a pointer to 
            /// the node that should be used to travel to this one. 
            public readonly int[] MinimumPath;

            /// The minimum distance from the starting node to the given node. 
            public readonly float[] MinimumDistance;
        }

        public class QueueElement : IComparable
        {
            public int index;
            public float weight;

            public QueueElement() { }
            public QueueElement(int i, float val)
            {
                index = i;
                weight = val;
            }

            public int CompareTo(object obj)
            {
                QueueElement outer = (QueueElement)obj;

                if (this.weight > outer.weight)
                    return 1;
                else if (this.weight < outer.weight)
                    return -1;
                else return 0;
            } 
        }


        // start: The node to use as a starting location. 
        // A struct containing both the minimum distance and minimum path 
        // to every node from the given <paramref name="start"/> node. 
        public virtual Results Perform(int start)
        {
            // Initialize the distance to every node from the starting node. 
            float[] d = GetStartingTraversalCost(start);

            // Initialize best path to every node as from the starting node. 
            int[] p = GetStartingBestPath(start);
            BasicHeap Q = new BasicHeap();
            //FastHeap Q = new FastHeap(TotalNodeCount);

            for (int i = 0; i != TotalNodeCount; i++)
                Q.Push(i, d[i]);

            while (Q.Count != 0)
            {
                int v = Q.Pop();
                foreach (int w in Hint(v))
                {
                    if (w < 0 || w > Q.Count - 1) continue;

                    float cost = TraversalCost(v, w);
                    if (cost < float.MaxValue && d[v] + cost < d[w]) // don't let wrap-around negatives slip by 
                    {
                        // We have found a better way to get at relative 
                        d[w] = d[v] + cost; // record new distance 
                        p[w] = v;
                        Q.Push(w, d[w]);
                    }
                }
            }

            return new Results(p, d);
        }

        // start: The node to use as a starting location. 
        // A struct containing both the minimum distance and minimum path 
        // to every node from the given <paramref name="start"/> node. 
        public virtual Results Perform2(int start)
        {
            // Initialize the distance to every node from the starting node. 
            float[] d = GetStartingTraversalCost(start);

            // Initialize best path to every node as from the starting node. 
            int[] p = GetStartingBestPath(start);
            BinaryPriorityQueue Q = new BinaryPriorityQueue();

            for (int i = 0; i != TotalNodeCount; i++)
                Q.Push(new QueueElement(i,d[i]));

            while (Q.Count!=0)
            {
                int v = ((QueueElement)Q.Pop()).index;
             
                foreach (int w in Hint(v))
                {
                    if (w <0 || w > Q.Count-1) continue;

                    float cost = TraversalCost(v, w);
                    if (cost < float.MaxValue && d[v] + cost < d[w]) // don't let wrap-around negatives slip by 
                    {
                        // We have found a better way to get at relative 
                        d[w] = d[v] + cost; // record new distance 
                        p[w] = v;
                        Q.Push(new QueueElement(w, d[w]));
                    }
                }
            }

            return new Results(p, d);
        }

        // Uses the Dijkstra algorithhm to find the minimum path 
        // from one node to another. 
        // Return a struct containing both the minimum distance and minimum path 
        // to every node from the given start node. 
        public virtual int[] GetMinimumPath(int start, int finish)
        {
            if (start < finish)
            {
                int tmp = start;
                start = finish;
                finish = tmp;
            }

            Results results = Perform(start);
            return GetMinimumPath(start, finish, results.MinimumPath);
        }

        // Finds an array of nodes that provide the shortest path 
        // from one given node to another. 
        // ShortestPath : P array of the completed algorithm:
        // The list of nodes that provide the one step at a time path from 
        protected virtual int[] GetMinimumPath(int start, int finish, int[] shortestPath)
        {
            Stack<int> path = new Stack<int>();

            do
            {
                path.Push(finish);
                finish = shortestPath[finish]; // step back one step toward the start point 
            }
            while (finish != start);
            return path.ToArray();
        }

        // Initializes the P array for the algorithm. 
        // A fresh P array will set every single node's source node to be  
        // the starting node, including the starting node itself. 
        protected virtual int[] GetStartingBestPath(int startingNode)
        {
            int[] p = new int[TotalNodeCount];
            for (int i = 0; i < p.Length; i++)
                p[i] = startingNode;
            return p;
        }

        // Initializes the D array for the start of the algorithm.
        // The traversal cost for every node will be set to impossible 
        // (int.MaxValue) unless a connecting edge is found between the 
        // starting node and the node in question.
        protected virtual float[] GetStartingTraversalCost(int start)
        {
            float[] subset = new float[TotalNodeCount];
            for (int i = 0; i != subset.Length; i++)
                subset[i] = float.MaxValue; // all are unreachable 
            subset[start] = 0; // zero cost from start to start 
            foreach (int nearby in Hint(start))
                subset[nearby] = TraversalCost(start, nearby);
            return subset;
        }

    }
}