using System.Collections.Generic;
using BusJam.Grid;
using UnityEngine;

namespace BusJam.Pathfinding
{
    public class AStar
    {
        private const int STRAIGHT_COST = 10;

        private static readonly Vector2Int[] DIRECTIONS = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        private Dictionary<Vector2Int, IGridElement> _nodeMap;

        public AStar()
        {
            _nodeMap = new Dictionary<Vector2Int, IGridElement>();
        }

        public void SetNodes(List<IGridElement> nodesList)
        {
            _nodeMap.Clear();

            for (int i = 0; i < nodesList.Count; i++)
            {
                var node = nodesList[i];
                _nodeMap[node.GridPosition] = node;
            }
        }

        public void ClearNodes()
        {
            _nodeMap.Clear();
        }

        public List<IGridElement> FindPath(IGridElement start, IGridElement end)
        {
            if (start == null || end == null)
            {
                return null;
            }

            var openSet = new List<Vector2Int>();
            var closedSet = new HashSet<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int>();
            var fScore = new Dictionary<Vector2Int, int>();

            var startPos = start.GridPosition;
            var endPos = end.GridPosition;

            openSet.Add(startPos);
            gScore[startPos] = 0;
            fScore[startPos] = Heuristic(startPos, endPos);

            while (openSet.Count > 0)
            {
                var currentPos = GetLowestFScoreNode(openSet, fScore);

                if (currentPos == endPos)
                {
                    return ReconstructPath(cameFrom, currentPos);
                }

                openSet.Remove(currentPos);
                closedSet.Add(currentPos);

                var neighborPositionsList = GetNeighborPositions(currentPos);

                for (int i = 0; i < neighborPositionsList.Count; i++)
                {
                    var neighborPos = neighborPositionsList[i];

                    if (closedSet.Contains(neighborPos))
                    {
                        continue;
                    }

                    if (!_nodeMap.ContainsKey(neighborPos))
                    {
                        continue;
                    }

                    var neighborNode = _nodeMap[neighborPos];

                    if (neighborPos != startPos && !neighborNode.IsWalkable)
                    {
                        continue;
                    }

                    var tentativeGScore = gScore[currentPos] + STRAIGHT_COST;

                    var currentGScore = int.MaxValue;
                    if (gScore.ContainsKey(neighborPos))
                    {
                        currentGScore = gScore[neighborPos];
                    }

                    if (tentativeGScore < currentGScore)
                    {
                        cameFrom[neighborPos] = currentPos;
                        gScore[neighborPos] = tentativeGScore;
                        fScore[neighborPos] = tentativeGScore + Heuristic(neighborPos, endPos);

                        if (!openSet.Contains(neighborPos))
                        {
                            openSet.Add(neighborPos);
                        }
                    }
                }
            }

            return null;
        }

        public List<IGridElement> FindPathToNearest(IGridElement start, List<IGridElement> targetsList)
        {
            if (start == null || targetsList == null || targetsList.Count == 0)
            {
                return null;
            }

            List<IGridElement> shortestPath = null;
            var shortestLength = int.MaxValue;

            for (int i = 0; i < targetsList.Count; i++)
            {
                var path = FindPath(start, targetsList[i]);

                if (path == null)
                {
                    continue;
                }

                if (path.Count < shortestLength)
                {
                    shortestLength = path.Count;
                    shortestPath = path;
                }
            }

            return shortestPath;
        }

        public bool IsPathAvailable(IGridElement start, List<IGridElement> targetsList)
        {
            var path = FindPathToNearest(start, targetsList);
            return path != null;
        }

        private int Heuristic(Vector2Int a, Vector2Int b)
        {
            var xDist = Mathf.Abs(a.x - b.x);
            var yDist = Mathf.Abs(a.y - b.y);
            return (xDist + yDist) * STRAIGHT_COST;
        }

        private List<Vector2Int> GetNeighborPositions(Vector2Int position)
        {
            var neighborsList = new List<Vector2Int>();

            for (int i = 0; i < DIRECTIONS.Length; i++)
            {
                var neighborPos = position + DIRECTIONS[i];

                if (_nodeMap.ContainsKey(neighborPos))
                {
                    neighborsList.Add(neighborPos);
                }
            }

            return neighborsList;
        }

        private List<IGridElement> ReconstructPath(
            Dictionary<Vector2Int, Vector2Int> cameFrom,
            Vector2Int current)
        {
            var pathList = new List<IGridElement>();
            pathList.Add(_nodeMap[current]);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                pathList.Add(_nodeMap[current]);
            }

            pathList.Reverse();

            if (pathList.Count > 0)
            {
                pathList.RemoveAt(0);
            }

            return pathList;
        }

        private Vector2Int GetLowestFScoreNode(
            List<Vector2Int> openSet,
            Dictionary<Vector2Int, int> fScore)
        {
            var lowestPos = openSet[0];
            var lowestFScore = int.MaxValue;

            if (fScore.ContainsKey(lowestPos))
            {
                lowestFScore = fScore[lowestPos];
            }

            for (int i = 1; i < openSet.Count; i++)
            {
                var pos = openSet[i];
                var score = int.MaxValue;

                if (fScore.ContainsKey(pos))
                {
                    score = fScore[pos];
                }

                if (score < lowestFScore)
                {
                    lowestFScore = score;
                    lowestPos = pos;
                }
            }

            return lowestPos;
        }
    }
}
