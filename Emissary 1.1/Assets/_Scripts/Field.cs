using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets._Scripts
{
    public class Field : MonoBehaviour
    {
        static double rootTwo = Mathf.Sqrt(2f);
        public Vector2 worldSize = Vector2.one;
        private Vector3 lastTarget;
        public float nodeRadius = 1;
        public LayerMask unwalkableMask;
        public TerrainType[] walkableRegions;
        public Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        public Dictionary<Vector3, Grid> gridDict;
        public List<Vector3> ActiveTargets;
        public static int distanceFactor = 10;
        public static Field instance;
        // Use this for initialization
        public Grid.GizmoDisplay DisplayGizmos = Grid.GizmoDisplay.SHOW;
        public Grid dGrid = null;
        public Grid defaultGrid
        {
            get
            {
                if (dGrid == null)
                {
                    //These checks are done when initializing the default grid.
                    //If the world size or node radius sizes are out of bounds, then they will be corrected.
                    if(worldSize.x <= 0 || worldSize.y <= 0)
                    {
                        worldSize = Vector2.one;
                        Debug.Log("WARNING: world size must be greater than 0 in both X and Y. Re-sizing to Vector2.one.");
                    }
                    if(nodeRadius <= 0)
                    {
                        nodeRadius = 1;
                        Debug.Log("WARNING: Node radius must be greater than 0. Re-sizing to 1.");
                    }
                    dGrid = new Grid(transform.position, worldSize, nodeRadius, walkableRegions, walkableRegionsDictionary, unwalkableMask);
                    Debug.Log("Default Grid Successfully Initialized.");
                    dGrid.target = Vector3.zero;


                }
                return dGrid;
            }
        }

        void Awake()
        {
            //Setting global static instance for external Field reference. There should only be one Field in the project. There may be problems otherwise.
            instance = this;
            //Set up the global grid pathing dictionary
            gridDict = new Dictionary<Vector3, Grid>();
            //Set up list of active grids
            ActiveTargets = new List<Vector3>();
            //Initializing 
            GenerateDictionaryDefinition(Vector3.zero);
            UpdateValues(Vector3.zero);
        }

        public Vector3 standardizedLocation(Vector3 location)
        {
            return defaultGrid.GetNodeFromWorldPoint(location).globalPosition;
        }

        public void GenerateDictionaryDefinition(Vector3 location)
        {
            
            ActiveTargets.Add(location);
            gridDict.Add(location, new Grid(transform.position, worldSize, nodeRadius, walkableRegions, walkableRegionsDictionary, unwalkableMask));
            Debug.Log("Grid Dictionary Initialization success: " + gridDict.ContainsKey(location));
            gridDict[location].target = location;
            Debug.Log(gridDict[location].target);
            //Debug.Log(gridDict[location]);
        }

        public IEnumerator UpdateValues(Vector3 Target)
        {
            lastTarget = Target;
            gridDict[Target].target = Target;
            //Grid grid = gridDict[target];
            float floorX = Target.x - nodeRadius;
            float ceilX = Target.x + nodeRadius;
            float floorZ = Target.z - nodeRadius;
            float ceilZ = Target.z + nodeRadius;
            if (floorX == ceilX)
            {
                Debug.Log("The X's are the same!");
            }
            if (floorZ == ceilZ)
            {
                Debug.Log("The Z's are the same!");
            }

            Queue<Node> openSet = new Queue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            foreach (Region region in gridDict[Target].regions)
            {
                region.oriented = true;
            }
            Node targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(floorX, 0, floorZ));
            //Debug.Log(targetNode);
            targetNode.gCost = 0;
            openSet.Enqueue(targetNode);
            targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(floorX, 0, ceilZ));
            if (!openSet.Contains(targetNode))
            {
                targetNode.gCost = 0;
                openSet.Enqueue(targetNode);
            }
            targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(ceilX, 0, floorZ));
            if (!openSet.Contains(targetNode))
            {
                targetNode.gCost = 0;
                openSet.Enqueue(targetNode);
            }
            targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(ceilX, 0, ceilZ));
            if (!openSet.Contains(targetNode))
            {
                targetNode.gCost = 0;
                openSet.Enqueue(targetNode);
            }
            yield return null;
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.Dequeue();
                closedSet.Add(currentNode);
                foreach (Node node in gridDict[Target].GetAdjacentNodes(currentNode))
                {

                    //Debug.Log(node);
                    if (node.walkable)
                    {
                        int cost = currentNode.gCost + distanceFactor;
                        if (!closedSet.Contains(node) || node.gCost >= cost)
                        {
                            //node.parent = currentNode;
                            node.gCost = cost + node.movementPenalty;
                            //Debug.Log(node.gCost);
                            if (!openSet.Contains(node) && !closedSet.Contains(node))
                            {
                                openSet.Enqueue(node);
                            }
                        }
                    }
                }
            }
            yield return null;
            OrientGrid(gridDict[Target], closedSet);
            yield return null;
            /*foreach (Unit unit in gridDict[Target].assignedUnits)
            {
                unit.InitiateMovement();
            }*/
        }

        public IEnumerator UpdateValues(Vector3 Target, Vector3[] path)
        {
            lastTarget = Target;
            gridDict[Target].target = Target;
            List<Region> regions = gridDict[Target].listRegionsAlongPath(path);
            float floorX = Target.x;
            float ceilX = Target.x + nodeRadius * 2.5f;
            float floorZ = Target.z;
            float ceilZ = Target.z + nodeRadius * 2.5f;
            if (floorX == ceilX)
            {
                Debug.Log("The X's are the same!");
            }
            if (floorZ == ceilZ)
            {
                Debug.Log("The Z's are the same!");
            }

            Queue<Node> openSet = new Queue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            Node targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(floorX, 0, floorZ));
            //Debug.Log(targetNode);
            targetNode.gCost = 0;
            openSet.Enqueue(targetNode);
            //finding the four 
            /*
            targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(floorX, 0, ceilZ));
                targetNode.gCost = 0;
                openSet.Enqueue(targetNode);
            targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(ceilX, 0, floorZ));
                targetNode.gCost = 0;
                openSet.Enqueue(targetNode);
            targetNode = gridDict[Target].GetNodeFromWorldPoint(new Vector3(ceilX, 0, ceilZ));
                targetNode.gCost = 0;
                openSet.Enqueue(targetNode);*/
            yield return null;

            foreach (Region region in gridDict[Target].listRegionsAlongPath(path))
            {
                region.oriented = true;
            }
            yield return null;
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.Dequeue();
                closedSet.Add(currentNode);

                currentNode.OnEdge = false;


                foreach (Node node in gridDict[Target].GetNeighborNodes(currentNode))
                {
                    if (!node.walkable || !(regions.Contains(node.region) || node.region.oriented))
                    {
                        currentNode.OnEdge = true;
                        break;
                    }
                }

                if (currentNode.GridPosition.x == 0 || currentNode.GridPosition.y == 0 || currentNode.GridPosition.x >= defaultGrid.gridSizeX - 1 || currentNode.GridPosition.y >= defaultGrid.gridSizeY - 1)
                    currentNode.OnEdge = true;
                if (currentNode.OnEdge)
                {
                    currentNode.gCost += distanceFactor;
                }

                foreach (Node node in gridDict[Target].GetAdjacentNodes(currentNode))
                {
                    //Debug.Log(node);
                    if (node.walkable && (regions.Contains(node.region) || node.region.oriented))
                    {
                        int cost = currentNode.gCost + distanceFactor;// + ((currentNode.OnEdge)?:);
                        if (!closedSet.Contains(node) || node.gCost > cost)
                        {
                            node.parent = currentNode;
                            node.gCost = cost + node.movementPenalty;
                            //Debug.Log(node.gCost);
                            if (!openSet.Contains(node) && !closedSet.Contains(node))
                            {
                                openSet.Enqueue(node);
                            }
                        }
                    }
                }
            }
            yield return null;
            OrientGrid(gridDict[Target], closedSet);
            yield return null;
            //Debug.Log(gridDict[location]);
            /*Debug.Log(gridDict[Target].assignedUnits.Count + " Unit" + ((gridDict[Target].assignedUnits.Count == 1) ? "" : "s") + " assigned to this grid.");
            Debug.Log("Sending Unit!");
            foreach (Unit unit in gridDict[Target].assignedUnits)
            {
                unit.InitiateMovement();
            }*/

        }

        private static void OrientGrid(Grid grid, HashSet<Node> closedSet)
        {
            //TODO: Region-based processing
            for (int x = 0; x < grid.gridSizeX; x++)
            {
                for (int y = 0; y < grid.gridSizeY; y++)
                {
                    if (closedSet.Contains(grid.GetNode(x, y)))
                    {
                        int up, down, left, right;
                        int upY, downY, leftX, rightX;
                        leftX = (x >= 1 && grid.GetNode(x - 1, y).region.oriented && grid.GetNode(x - 1, y).walkable) ? x - 1 : x;
                        rightX = (x < grid.gridSizeX - 1 && grid.GetNode(x + 1, y).region.oriented && grid.GetNode(x + 1, y).walkable) ? x + 1 : x;
                        upY = (y >= 1 && grid.GetNode(x, y - 1).region.oriented && grid.GetNode(x, y - 1).walkable) ? y - 1 : y;
                        downY = (y < grid.gridSizeY - 1 && grid.GetNode(x, y + 1).region.oriented && grid.GetNode(x, y + 1).walkable) ? y + 1 : y;

                        up = grid.GetNode(x, upY).gCost;
                        down = grid.GetNode(x, downY).gCost;
                        left = grid.GetNode(leftX, y).gCost;
                        right = grid.GetNode(rightX, y).gCost;
                        float factor = 1f / Mathf.Sqrt(4 * (Mathf.Pow(left - right, 2) + Mathf.Pow(down - up, 2)));
                        Vector3 direction = new Vector3(left - right, 0f, up - down) * factor;
                        grid.GetNode(x, y).flowDirection = direction;

                    }
                }
            }
        }

        public static int GetDist(Node nodeA, Node nodeB)
        //Returns the general distance between two nodes.
        {
            int distX = (int)Mathf.Abs(nodeA.GridPosition.x - nodeB.GridPosition.x);
            int distY = (int)Mathf.Abs(nodeA.GridPosition.y - nodeB.GridPosition.y);
            if (distX > distY)
            {
                return (int)(distanceFactor * rootTwo * distY + 10 * (distX - distY));
            }
            return (int)(distanceFactor * rootTwo * distX + 10 * (distY - distX));
        }

        void OnDrawGizmos()
        {
            switch (DisplayGizmos)
            {
                case Grid.GizmoDisplay.FORCE:
                case Grid.GizmoDisplay.SHOW:
                    if (gridDict != null && gridDict.Count != 0)
                    {
                        gridDict[lastTarget].DrawGizmos(DisplayGizmos);
                    }
                    else
                    {
                        defaultGrid.DrawGizmos(DisplayGizmos);
                    } 
                
                    break;
                case Grid.GizmoDisplay.HIDE:
                    break;
                

            }


        }
    }

}

