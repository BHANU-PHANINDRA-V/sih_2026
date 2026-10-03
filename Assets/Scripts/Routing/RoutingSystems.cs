using System.Collections.Generic;
using UnityEngine;

namespace TrinetraKavach.Routing
{
    public class GridNode
    {
        public int GridX;
        public int GridY;
        public Vector3 WorldPosition;
        public bool IsWalkable;
        public bool IsHazard;
        public int MovementCost;

        public int G;
        public int H;
        public int F => G + H;
        public GridNode Parent;

        public GridNode(int x, int y, Vector3 worldPos, bool walkable)
        {
            GridX = x;
            GridY = y;
            WorldPosition = worldPos;
            IsWalkable = walkable;
            IsHazard = false;
            MovementCost = 1;
        }
    }

    public class AStarPathfinder : MonoBehaviour
    {
        public static AStarPathfinder Instance { get; private set; }

        private void Awake() => Instance = this;

        public List<GridNode> FindPath(GridNode startNode, GridNode targetNode, GridManager grid)
        {
            List<GridNode> openSet = new List<GridNode>();
            HashSet<GridNode> closedSet = new HashSet<GridNode>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                GridNode current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].F < current.F || (openSet[i].F == current.F && openSet[i].H < current.H))
                    {
                        current = openSet[i];
                    }
                }

                openSet.Remove(current);
                closedSet.Add(current);

                if (current == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (GridNode neighbor in grid.GetNeighbors(current))
                {
                    if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                        continue;

                    int newCost = current.G + GetDistance(current, neighbor) + neighbor.MovementCost;
                    if (newCost < neighbor.G || !openSet.Contains(neighbor))
                    {
                        neighbor.G = newCost;
                        neighbor.H = GetDistance(neighbor, targetNode);
                        neighbor.Parent = current;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
        {
            List<GridNode> path = new List<GridNode>();
            GridNode current = endNode;
            while (current != startNode)
            {
                path.Add(current);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        private int GetDistance(GridNode a, GridNode b)
        {
            int dstX = Mathf.Abs(a.GridX - b.GridX);
            int dstY = Mathf.Abs(a.GridY - b.GridY);
            if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }

    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        public int Width = 10;
        public int Height = 10;
        public float NodeSize = 0.5f;
        private GridNode[,] nodes;

        private void Awake()
        {
            Instance = this;
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            nodes = new GridNode[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector3 worldPos = new Vector3(x * NodeSize, 0, y * NodeSize);
                    nodes[x, y] = new GridNode(x, y, worldPos, true);
                }
            }
        }

        public void MarkHazard(int x, int y, bool isHazard)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                nodes[x, y].IsHazard = isHazard;
                nodes[x, y].IsWalkable = !isHazard;
                nodes[x, y].MovementCost = isHazard ? 999 : 1;
            }
        }

        public List<GridNode> GetNeighbors(GridNode node)
        {
            List<GridNode> neighbors = new List<GridNode>();
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int checkX = node.GridX + dx[i];
                int checkY = node.GridY + dy[i];

                if (checkX >= 0 && checkX < Width && checkY >= 0 && checkY < Height)
                {
                    neighbors.Add(nodes[checkX, checkY]);
                }
            }
            return neighbors;
        }

        public GridNode GetNode(int x, int y) => nodes[x, y];
    }

    public class RoutingManager : MonoBehaviour
    {
        public static RoutingManager Instance { get; private set; }
        [SerializeField] private ARRouteRenderer routeRenderer;

        private void Awake() => Instance = this;

        public void CalculateAndShowSafeRoute(Vector3 startPos, Vector3 exitPos)
        {
            GridNode start = GridManager.Instance.GetNode(0, 0);
            GridNode exit = GridManager.Instance.GetNode(GridManager.Instance.Width - 1, GridManager.Instance.Height - 1);

            List<GridNode> path = AStarPathfinder.Instance.FindPath(start, exit, GridManager.Instance);
            if (routeRenderer != null && path != null)
            {
                routeRenderer.RenderRoute(path);
            }
        }
    }

    public class ARRouteRenderer : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.cyan;
        }

        public void RenderRoute(List<GridNode> path)
        {
            if (path == null) return;
            lineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                lineRenderer.SetPosition(i, path[i].WorldPosition + Vector3.up * 0.05f);
            }
        }
    }
}
