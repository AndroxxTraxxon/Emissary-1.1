﻿using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets._Scripts
{
    public class Region
    {
        public bool displayGridGizmos = true;
        // Use this for initialization
        public const int STANDARD_SIZE = 16;
        Grid parentGrid;
        int width, height;
        Vector2 gridCoords;
        internal Node[,] nodes;// = new Node[width, height];
        Vector3 offset;
        float nodeScale;
        float nodeRadius;
        Vector3 RegionBottomLeft;
        public bool oriented
        {
            get;
            set;
        }
        //public List<Unit> assignedUnits;

        public Region(Grid parentGrid)
        {
            gridCoords = Vector2.zero;
            width = STANDARD_SIZE;
            height = STANDARD_SIZE;
            nodes = new Node[width, height];
            nodeScale = 1;
            nodeRadius = 1f / 2f;
            offset = new Vector3();
            this.parentGrid = parentGrid;
            RegionBottomLeft = offset - Vector3.right * width * nodeRadius - Vector3.forward * height * nodeRadius;
            InitializeNodes();
        }


        public Region(int x, int y, int width, int height, float nodeScale, Vector3 offset, Grid parentGrid)
        {
            gridCoords.x = x;
            gridCoords.y = y;
            this.width = width;
            this.height = height;
            nodes = new Node[width, height];
            this.nodeScale = nodeScale;
            nodeRadius = nodeScale / 2f;
            this.offset = offset;
            //Debug.Log(offset);
            this.parentGrid = parentGrid;
            //Debug.Log(nodeRadius);
            RegionBottomLeft = this.offset - Vector3.right * width * nodeRadius - Vector3.forward * height * nodeRadius;
            InitializeNodes();
        }

        private void InitializeNodes()
        {
            //Debug.Log("Initializing Nodes: width " + width + ", height " + height);
            //Debug.Log(RegionBottomLeft);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector3 worldPoint = RegionBottomLeft + Vector3.right * (i * nodeScale + nodeRadius) + Vector3.forward * (j * nodeScale + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeScale / 2, parentGrid.unwalkableMask));
                    int movementPenalty = 0;
                    if (walkable)
                    {
                        Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100, parentGrid.walkableMask))
                        {
                            parentGrid.walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        }
                    }
                    nodes[i, j] = new Node(walkable, worldPoint, i, j, movementPenalty, this);
                    //Debug.Log("New Node: " + nodes[i, j].ToString());
                }
            }
        }

        public Node getNode(int x, int y)
        {
            return nodes[x, y];
        }

        

        internal Grid GetParentGrid()
        {
            return parentGrid;
        }

        public override string ToString()
        {
            return "Region: " + width + ", " + height;
        }

        internal Vector2 GridCoords
        {
            get
            {
                return gridCoords;
            }
        }
        internal void DrawGizmos(Grid.GizmoDisplay gizmoMode)
        {

            if (gizmoMode != Grid.GizmoDisplay.HIDE)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(offset, Vector3.right * width * nodeScale + Vector3.forward * height * nodeScale);

                if (nodes != null)
                {
                    if (gizmoMode != Grid.GizmoDisplay.MIN && (oriented || gizmoMode == Grid.GizmoDisplay.FORCE))
                    {
                        foreach (Node n in nodes)
                        {
                            //Baseline: Walkable --> white; Not Walkable --> red
                            Gizmos.color = new Color(1 - n.gCost / 255f, 1 - n.gCost / 255f, 1);
                            if (!n.walkable) Gizmos.color = Color.red;
                            else if (n.OnEdge) Gizmos.color = Color.green;
                            else if (n.gCost == 0) Gizmos.color = Color.cyan;
                            //Player Position --> cyan
                            Gizmos.DrawCube(n.globalPosition, Vector3.one * (nodeScale * .195f));
                            Gizmos.DrawRay(n.globalPosition, n.flowDirection * nodeScale);
                        }

                    }
                }
            }

        }

        public static T[,] ResizeArray<T>(T[,] original, int rows, int cols)
        {
            var newArray = new T[rows, cols];
                int minRows = Math.Min(rows, original.GetLength(0));
                int minCols = Math.Min(cols, original.GetLength(1));
            for(int i = 0; i<minRows; i++)
                for(int j = 0; j<minCols; j++)
                    newArray[i, j] = original[i, j];
            return newArray;
        }
    }
}