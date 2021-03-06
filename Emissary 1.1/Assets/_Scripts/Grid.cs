﻿using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets._Scripts
{
    public class Grid
    {
        //public Transform player;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float NodeRadius;
        public TerrainType[] walkableRegions;
        internal LayerMask walkableMask;
        internal Region[,] regions;
        public Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        Vector3 RegionStandardSize;
        Vector3 position;
        public Vector3 target;
        float NodeDiameter;
        private int gridX, gridY;
        //private bool initialized = false;
        public enum GizmoDisplay {HIDE, MIN_DEF, MIN ,DEFAULT, SHOW, FORCE};

        private static Vector2[] neighborNodeLocs = {
            new Vector2(-1,1), new Vector2(0,1), new Vector2(1,1),
            new Vector2(-1,0), /*skip   middle*/ new Vector2(0,1),
            new Vector2(-1,-1), new Vector2(0,-1), new Vector2(1,-1)
        };
        private static Vector2[] adjNodeLocs =  {
            /*skip the corner*/ new Vector2(0,1), /*skip corner*/
            new Vector2(-1,0), /*skip   middle*/ new Vector2(1,0),
            /*skip the corner*/ new Vector2(0,-1) /*skip corner*/
        };


        public int gridSizeX
        {
            get
            {
                return gridX;
            }
        }
        public int gridSizeY
        {
            get
            {
                return gridY;
            }
        }

        List<Region> regionList;
        //public GizmoDisplay displayGizmos = GizmoDisplay.HIDE;//this is only for use on the default grid
        //public List<Unit> assignedUnits;


        public Grid(Vector3 position, Vector2 gridSize, float nodeRadius, TerrainType[] walkableAreas, Dictionary<int, int> walkRegDict, LayerMask unwalkableMask)
        {

            //taking in the input.x
            this.position = position;
            gridWorldSize = gridSize;
            NodeRadius = nodeRadius;
            
            walkableRegions = walkableAreas;
            walkableRegionsDictionary = walkRegDict;
            this.unwalkableMask = unwalkableMask;

            CreateGrid();
        }

        public int MaxSize
        {
            get
            {
                return gridX * gridY;
            }
        }

        public bool TryGetNearestWalkableNode(Vector3 worldPosition, out Node node)
        {
            node = GetNodeFromWorldPoint(worldPosition);
            if (node.walkable)
            {
                return true;
            }
            else
            {
                Queue<Node> nodes = new Queue<Node>();
                foreach (Node vn in GetAdjacentNodes(node))
                {
                    nodes.Enqueue(vn);
                }
                int radius = 2;//this value will change how far the unit looks for neighboring nodes. 
                int count = 0;
                while (nodes.Count > 0 && count++ < (Mathf.Pow(2 * radius + 1, 2) - 1))
                {
                    node = nodes.Dequeue();
                    if (node.walkable)
                    {
                        return true;
                    }

                    foreach (Node vn in GetAdjacentNodes(node))
                    {
                        nodes.Enqueue(vn);
                    }
                }
            }
            Debug.Log("This node can't find a friend around here!");
            return false;
        }

        public bool TryGetNearestOrientedWalkableNode(Vector3 worldPosition, out Node node)
        {
            //get the "central node" 
            node = GetNodeFromWorldPoint(worldPosition);
            if (node.walkable && node.region.oriented)
            {
                return true;
            }
            else
            {
                Queue<Node> nodes = new Queue<Node>();
                foreach (Node vn in GetAdjacentNodes(node))
                {
                    nodes.Enqueue(vn);
                }
                int radius = 2;//this value will change how far the unit looks for neighboring nodes. 
                int count = 0;
                while (nodes.Count > 0 && count++ < (Mathf.Pow(2 * radius + 1, 2) - 1))
                {
                    node = nodes.Dequeue();
                    if (node.walkable && node.region.oriented)
                    {
                        return true;
                    }

                    foreach (Node vn in GetAdjacentNodes(node))
                    {
                        nodes.Enqueue(vn);
                    }
                }
            }
            Debug.Log("This node can't find a friend around here!");
            return false;
        }

        public Node GetNodeFromWorldPoint(Vector3 worldPosition)
        //returns a Node given a global position.
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridX - 1) * percentX);
            int y = Mathf.RoundToInt((gridY - 1) * percentY);
            //Debug.Log("Grid Position: " + x + ", " + y);
            return GetNode(x, y);
        }

        public Node GetNode(int x, int y)
        //returns node at grid location x,y 
        {
            int regionX = x / Region.STANDARD_SIZE;
            int regionY = y / Region.STANDARD_SIZE;
            x = x % Region.STANDARD_SIZE;
            y = y % Region.STANDARD_SIZE;
            //Debug.Log("Region Position: " + x + ", " + y);
            // Debug.Log(regions[regionX, regionY].getNode(x, y));
            return this.regions[regionX, regionY].getNode(x, y);
        }

        public Node GetNode(float x, float y)
        {
            return GetNode(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }

        public Node GetNode(Vector2 Position)
        {
            return GetNode(Position.x, Position.y);
        }
        
        public List<Node> getNodeQuad(Vector3 worldPosition)
        {
            List<Node> nodes = new List<Node>();

            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int xMax = (int)Mathf.Ceil((gridX - 1) * percentX);
            int yMax = (int)Mathf.Ceil((gridY - 1) * percentY);
            int xMin = (int)Mathf.Floor((gridX - 1) * percentX);
            int yMin = (int)Mathf.Floor((gridX - 1) * percentY);
            if(xMax == xMin)
            {
                if(percentX > .5f)
                {
                    xMin--;
                }
                else
                {
                    xMax++;
                }
            }
            if(yMax == yMin)
            {
                if(percentY > .5f)
                {
                    yMin--;
                }
                else
                {
                    yMax++;
                }
            }
            //Debug.Log("Grid Position: " + x + ", " + y);
            nodes.Add(GetNode(xMax, yMax));
            nodes.Add(GetNode(xMax, yMin));
            nodes.Add(GetNode(xMin, yMax));
            nodes.Add(GetNode(xMin, yMin));
            return nodes;

        }
        public List<Node> getNodeQuad(Vector3 worldPosition, out Vector3 center)
        {
            List<Node> nodes = new List<Node>();

            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int xMax = (int)Mathf.Ceil((gridX - 1) * percentX);
            int yMax = (int)Mathf.Ceil((gridY - 1) * percentY);
            int xMin = (int)Mathf.Floor((gridX - 1) * percentX);
            int yMin = (int)Mathf.Floor((gridX - 1) * percentY);
            if (xMax == xMin)
            {
                if (percentX > .5f)
                {
                    xMin--;
                }
                else
                {
                    xMax++;
                }
            }
            if (yMax == yMin)
            {
                if (percentY > .5f)
                {
                    yMin--;
                }
                else
                {
                    yMax++;
                }
            }
            //Debug.Log("Grid Position: " + x + ", " + y);
            nodes.Add(GetNode(xMax, yMax));
            nodes.Add(GetNode(xMax, yMin));
            nodes.Add(GetNode(xMin, yMax));
            nodes.Add(GetNode(xMin, yMin));
            center = Vector3.zero;
            foreach(Node n in nodes)
            {
                center += n.globalPosition;
            }
            center /= 4;

            return nodes;

        }

        public List<Node> GetNeighborNodes(Node node)
        //returns a list of the (up to) eight neighboring Nodes, adjacent and diagonal Nodes.
        {
            List<Node> nodes = new List<Node>();

            foreach(Vector2 dir in neighborNodeLocs)
            {
                Vector2 loc = dir + node.GridPosition;
                if (loc.x >= 0 && loc.x < gridX && loc.y >= 0 && loc.y < gridY)
                {
                    nodes.Add(GetNode(loc));
                }
            }

            return nodes;
        }

        public List<Node> GetAdjacentNodes(Node node)
        //returns the list of (up to) four adjacent nodes immediately up, down, left, and right of the input node
        {
            List<Node> nodes = new List<Node>();
            Vector2 loc;
            foreach(Vector2 dir in adjNodeLocs)
            {
                loc = dir + node.GridPosition;
                if (loc.x >= 0 && loc.x < gridX && loc.y >= 0 && loc.y < gridY)
                {
                    nodes.Add(GetNode(loc));
                }

            }
            return nodes;
        }

        public Node[] AdjacentReferences(Node node)
        {
            Node[] nodes = new Node[4];
            for(int i = 0; i < 4; i++)
            {
                Vector2 pos = adjNodeLocs[i] + node.GridPosition;
                //0:up || 1:left || 2:right ||3:down
                //   0
                // 1 n 2
                //   3
                if(pos.x >= 0 && pos.x < gridSizeX && pos.y >= 0 && pos.y < gridSizeY)
                {
                    if (GetNode(pos).walkable && GetNode(pos).region.oriented)
                    {
                        nodes[i] = GetNode(pos);
                    }
                    else
                    {
                        nodes[i] = node;
                    }
                }
                else
                {
                    nodes[i] = node;
                }
                
            }
            return nodes; 
        }

        public Node[] AdjacentReferences(Node node, out bool onEdge)
        {
            onEdge = false;
            Node[] nodes = new Node[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = adjNodeLocs[i] + node.GridPosition;
                //0:up || 1:left || 2:right ||3:down
                //   0
                // 1 n 2
                //   3
                if (pos.x >= 0 && pos.x < gridSizeX && pos.y >= 0 && pos.y < gridSizeY)
                {
                    if (GetNode(pos).walkable && GetNode(pos).region.oriented)
                    {
                        nodes[i] = GetNode(pos);
                    }
                    else
                    {
                        nodes[i] = node;
                        onEdge = true;
                    }
                }
                else
                {
                    nodes[i] = node;
                }

            }
            return nodes;
        }

        public List<Node> GetAdjacentWalkableOrientedNodes(Node node)
        {
            List<Node> nodes = new List<Node>();
            Vector2 loc;
            Node candidate;
            foreach (Vector2 dir in adjNodeLocs)
            {
                loc = dir + node.GridPosition;
                if (loc.x >= 0 && loc.x < gridX && loc.y >= 0 && loc.y < gridY)
                {
                    candidate = GetNode(loc);
                    if (candidate.walkable && candidate.region.oriented)
                        nodes.Add(candidate);
                }

            }
            return nodes;
        }


        void CreateGrid()
        //initializes the grid and determines if each Node is walkable, then positions the Nodes in worldspace.
        {
            //process the basic values based on what was input
            NodeDiameter = NodeRadius * 2;
            RegionStandardSize = new Vector3(Region.STANDARD_SIZE * NodeDiameter, 0, Region.STANDARD_SIZE * NodeDiameter);
            gridX = Mathf.RoundToInt(gridWorldSize.x / NodeDiameter);
            gridY = Mathf.RoundToInt(gridWorldSize.y / NodeDiameter);
            regionList = new List<Region>();
            //assignedUnits = new List<Unit>();

            if (walkableRegionsDictionary==null)
            {
                walkableRegionsDictionary = new Dictionary<int, int>();
            }

            foreach (TerrainType region in walkableRegions)
            {
                walkableMask.value |= region.terrainMask.value;
                walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
            }

            int rcx = (int)Mathf.Ceil((gridX * 1.0f) / (float)Region.STANDARD_SIZE);
            int rcy = (int)Mathf.Ceil((gridY * 1.0f) / (float)Region.STANDARD_SIZE);


            int lastWidth = Region.STANDARD_SIZE;
            int lastHeight = Region.STANDARD_SIZE;

            lastWidth = (gridX % Region.STANDARD_SIZE == 0) ? lastWidth : (gridX % Region.STANDARD_SIZE);
            lastHeight = (gridY % Region.STANDARD_SIZE == 0) ? lastHeight : (gridY % Region.STANDARD_SIZE);
            
            regions = new Region[rcx, rcy];
            
            Vector3 worldBottomLeft = position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < rcx; x++)
            {
                for (int y = 0; y < rcy; y++)
                {
                    int width = Region.STANDARD_SIZE;
                    int height = Region.STANDARD_SIZE;
                    if (x == rcx - 1)
                    {
                        width = lastWidth;
                    }

                    if (y == rcy - 1)
                    {
                        height = lastHeight;
                    }

                    float centerX = worldBottomLeft.x + width * NodeRadius + x * RegionStandardSize.x;
                    float centerZ = worldBottomLeft.z + height * NodeRadius + y * RegionStandardSize.z;

                    regions[x, y] = new Region(x, y, width, height, NodeDiameter, new Vector3(centerX, 0, centerZ), this);
                    regionList.Add(regions[x, y]);
                }

            }
        }

        public Region GetRegion(int x, int y)
        //returns the Region at x,y
        {
            return regions[x, y];
        }

        public Region GetRegionFromGridLocation(int x, int y)
        //returns the region that contains the node with location x,y
        {
            return GetNode(x, y).region;
        }

        public List<Region> listRegions()
        //returns a list of all regions in the grid
        {
            return regionList;
        }

        public List<Region> listRegionsAlongPath(Vector3[] path)
        //returns a list of all regions which contain nodes along the given path
        {
            List<Region> list = new List<Region>();
            foreach (Vector3 loc in path)
            {
                Node node = GetNodeFromWorldPoint(loc);
                if (!list.Contains(node.region))
                {
                    list.Add(node.region);
                }
            }

            return list;
        }

        public override string ToString()
        {
            return "V-Grid: Destination " + target;
        }

        public void DrawGizmos(GizmoDisplay displayGizmos)
        {
            switch(displayGizmos)
            {

                case GizmoDisplay.MIN:
                    foreach(Node n in getNodeQuad(target))
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireCube(n.globalPosition, Vector3.one * NodeRadius / 2f);
                    }
                    goto case GizmoDisplay.SHOW;
                case GizmoDisplay.SHOW:
                case GizmoDisplay.FORCE:
                    Gizmos.DrawWireCube(position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
                    foreach (Region region in regions)
                    {
                        region.DrawGizmos(displayGizmos);
                    }
                    break;
                
                default:
                    return;
            }
        }

    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

    
}