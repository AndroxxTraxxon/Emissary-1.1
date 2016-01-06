using UnityEngine;
using System.Collections;
using System;

namespace Assets._Scripts
{

    public class Node : IHeapItem<Node>
    {

        public bool walkable;
        public Vector3 globalPosition;// The "global" location, or absolute location of the Node. where it would be if it were a 
        public Vector2 regionCoords;//The regional location, or its position within its own region
        public Node parent; // For A* to keep track of where the path should take you. 
        public ArrayList children; // For flow field (there may need to be a better method), so that you know which nodes to update if this one is changed.
        public int movementPenalty; // Add this to cost to simulate sticky terrain or something. value may vary based on anything you want.
        private Vector3 flowDir = Vector3.zero;// for Flowfield.
        public Vector3 flowDirection
        {
            get
            {
                return flowDir;
            }
            set
            {
                flowDir = value;
                if (!walkable || gCost == 0)
                    flowDir = Vector3.zero;
                else if (flowDir == Vector3.one)
                {
                    //reprocess flowDir if you set it to null.
                    int up, down, left, right;

                    Node[] nodes = region.GetParentGrid().AdjacentReferences(this);



                    down = nodes[0].gCost;
                    left = nodes[1].gCost;
                    right = nodes[2].gCost;
                    up = nodes[3].gCost;
                    
                    
                    float factor = 1f / Mathf.Sqrt(4 * (Mathf.Pow(left - right, 2) + Mathf.Pow(down - up, 2)));
                    Vector3 direction = new Vector3(left - right, 0f, up - down) * factor;
                    flowDir = direction;
                }
            }
        }

        public Region region;
        public bool OnEdge;

        public int gCost;
        public int hCost;

        int heapIndex;

        public Node(bool _walkable, Vector3 _worldPos, int _regionX, int _regionY, int _movementPenalty, Region region)
            //constructor... kinda self-explanatory
        {
            walkable = _walkable;
            globalPosition = _worldPos;
            regionCoords = new Vector2(_regionX, _regionY);
            movementPenalty = _movementPenalty;
            this.region = region;
        }

        public int HeapIndex
            //for when you throw all of them into a heap.
        {
            get
            {
                return heapIndex;
            }

            set
            {
                heapIndex = value;
            }
        }

        public int CompareTo(Node other)
            //crucial for comparing nodes, making pathfinding possible.
        {
            int compare = gCost.CompareTo(other.gCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(other.hCost);
            }

            return -compare;
        }

        public override string ToString()
        {
            return "NODE{Walk: " + walkable + ", WorldPos: " + globalPosition + ", GridPos: " + GridPosition + "}";
        }

        public Vector2 GridPosition
            //returns the absolute grid position (NOT the region position) . useful when going across regions.
        {
            get
            {
                return region.GridCoords * Region.STANDARD_SIZE + regionCoords;
            }
        }
        
    }
}
