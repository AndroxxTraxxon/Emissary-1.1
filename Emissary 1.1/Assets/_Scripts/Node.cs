using UnityEngine;
using System.Collections;
using System;

namespace Assets._Scripts
{

    public class Node : IHeapItem<Node>
    {

        public bool walkable;
        public Vector3 globalPosition;
        public Vector2 regionCoords;
        public Node parent;
        public ArrayList children;
        public int movementPenalty;
        public Vector3 flowDirection;
        public Region region;
        public bool OnEdge;

        public int gCost;
        public int hCost;

        int heapIndex;

        public Node(bool _walkable, Vector3 _worldPos, int _regionX, int _regionY, int _movementPenalty, Region region)
        {
            walkable = _walkable;
            globalPosition = _worldPos;
            regionCoords = new Vector2(_regionX, _regionY);
            movementPenalty = _movementPenalty;
            this.region = region;
        }

        public int HeapIndex
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
            return "NODE{WK: " + walkable + ", WP: " + globalPosition + ", GP: " + GridPosition + "}";
        }

        public Vector2 GridPosition
        {
            get
            {
                return region.GridCoords * 16 + regionCoords;
            }
        }
    }
}
