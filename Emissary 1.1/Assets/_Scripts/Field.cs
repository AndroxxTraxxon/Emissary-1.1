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
        private Vector2 oldWS = Vector2.one;
        private Vector3 lastTarget;
        public float nodeRadius = 1;
        private float oldNR = 1; // for grid size validation purposes.
        public LayerMask unwalkableMask;
        public TerrainType[] walkableRegions;
        public Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        public Dictionary<Vector3, Grid> gridDict;
        //public List<Vector3> ActiveTargets;//I'm not sure exactly what this was for.
        public static int distanceFactor = 10;
        public static Field instance;
        // Use this for initialization
        public Grid.GizmoDisplay DisplayGizmos = Grid.GizmoDisplay.MIN_DEF;
        public Grid dGrid = null;
        public Grid defaultGrid
        {
            get
            {
                if (dGrid == null)
                {
                    //These checks are done when initializing the default grid.
                    //If the world size or node radius sizes are out of bounds, then they will be corrected.
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
            //ActiveTargets = new List<Vector3>();
            //Initializing 
            //GenerateDictionaryDefinition(Vector3.zero);
            //UpdateValues(Vector3.zero);
            StartCoroutine(OrientGrid(defaultGrid, Vector3.zero));
        }

        public void GenerateDictionaryDefinition(Vector3 location)
        {

            defaultGrid.getNodeQuad(location, out location);//standardizing the location of the target to the grid.
            gridDict.Add(location, new Grid(transform.position, worldSize, nodeRadius, walkableRegions, walkableRegionsDictionary, unwalkableMask));
            gridDict[location].target = location;
        }

        public IEnumerator OrientGrid(Grid grid, Vector3 target)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            defaultGrid.getNodeQuad(target, out target);
            grid.target = (lastTarget = target);
            
            Queue<Node> openSet = new Queue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            foreach (Region region in grid.regions)
            {
                region.oriented = true;
            }

            foreach (Node n in grid.getNodeQuad(target, out target))
            {
                if (n.walkable)
                {
                    n.gCost = 0;
                    openSet.Enqueue(n);
                    closedSet.Add(n);
                }

            }

            
            yield return null;
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.Dequeue();
                if(!closedSet.Contains(currentNode))
                    closedSet.Add(currentNode);
                foreach (Node node in grid.GetAdjacentNodes(currentNode))
                {
                    //Debug.Log(node);
                    if (node.walkable)
                    {
                        int cost = currentNode.gCost + distanceFactor;
                        if (!(closedSet.Contains(node)||openSet.Contains(node)))
                        {
                            //node.parent = currentNode;
                            node.gCost = cost;// + node.movementPenalty;
                            //Debug.Log(node.gCost);
                            if (!(openSet.Contains(node)))
                            {
                                openSet.Enqueue(node);
                            }
                        }
                    }
                }
            }
            yield return null;
            foreach(Node n in closedSet)
            {
                n.flowDirection = Vector3.one;//This value is used to trigger the processing for the node flowDirection. 
                yield return new WaitForEndOfFrame();
            }
            /*
            for (int x = 0; x < grid.gridSizeX; x++)
            {
                for (int y = 0; y < grid.gridSizeY; y++)
                {
                    if (closedSet.Contains(grid.GetNode(x, y)))
                    {
                        if(grid.GetNode(x,y).gCost == 0)
                        {
                            grid.GetNode(x, y).flowDirection = Vector3.zero;
                            continue;
                        }

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
                        //Debug.Log("Orienting " + grid.GetNode(x, y));
                    }
                }
            }*/
            //sw.Stop();
            //Debug.Log("Path found: " + sw.ElapsedMilliseconds + "ms.");
            yield return null;

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
                case Grid.GizmoDisplay.MIN:
                    if (gridDict != null && gridDict.Count != 0)
                    {
                        gridDict[lastTarget].DrawGizmos(DisplayGizmos);
                    }
                    else
                    {
                        defaultGrid.DrawGizmos(DisplayGizmos);
                    } 
                
                    break;
                case Grid.GizmoDisplay.DEFAULT:
                    defaultGrid.DrawGizmos(Grid.GizmoDisplay.SHOW);
                    break;
                case Grid.GizmoDisplay.MIN_DEF:
                    defaultGrid.DrawGizmos(Grid.GizmoDisplay.MIN);
                    break;
                case Grid.GizmoDisplay.HIDE:
                    break;
            }


        }

        void OnValidate()
        {
            if (nodeRadius <= 0)
            {
                nodeRadius = 1;
                Debug.Log("WARNING: Node radius must be greater than 0. Re-sizing to 1.");
                dGrid = null;
            }else if (nodeRadius!= oldNR) {
                oldNR = nodeRadius;
                dGrid = null;
            }
            if(worldSize.x <= 0 || worldSize.y <= 0)
            {
                worldSize = Vector2.one;
                Debug.Log("WARNING: World Size must be greater than 0. Re-sizing to <Vector2.one>.");
                dGrid = null;
            }
            else if (worldSize != oldWS)
            {
                oldWS = worldSize;
                dGrid = null;
            }


        }

    }

}

