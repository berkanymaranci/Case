using System.Collections.Generic;
using BusJam.Data;
using BusJam.Passengers;
using BusJam.Pathfinding;
using UnityEngine;

namespace BusJam.Grid
{
    public class LevelManager : MonoBehaviour
    {
        private const float CELL_SPACING = 1f;

        private static readonly Vector2Int[] DIRECTION_OFFSETS = new Vector2Int[]
        {
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0)
        };

        [SerializeField]
        private GridFactory gridFactory;

        [SerializeField]
        private PassengerFactory passengerFactory;

        private List<IGridElement> _gridElements;
        private List<ExitPoint> _exitPointsList;
        private List<IGridElement> _exitTargetsList;
        private List<Tunnel> _tunnelsList;
        private AStar _aStar;
        private int _gridWidth;
        private Vector3 _origin;

        public void CreateLevel(LevelData data)
        {
            _gridElements = new List<IGridElement>();
            _exitPointsList = new List<ExitPoint>();
            _tunnelsList = new List<Tunnel>();
            _aStar = new AStar();
            _gridWidth = data.gridWidth;
            _origin = CalculateGridOrigin(data.gridWidth);
            for (int y = 0; y < data.gridHeight; y++)
            {
                for (int x = 0; x < data.gridWidth; x++)
                {
                    var cellData = data.rows[y].cells[x];
                    if (cellData.isBlocked)
                    {
                        _gridElements.Add(null);
                        continue;
                    }
                    var position = _origin + new Vector3(x * CELL_SPACING, 0f, -y * CELL_SPACING);
                    var gridPos = new Vector2Int(x, y);
                    if (cellData.isTunnel)
                    {
                        _gridElements.Add(null);
                        continue;
                    }
                    var cell = gridFactory.GetCell(gridPos, false);
                    cell.transform.position = position;
                    cell.transform.rotation = Quaternion.identity;
                    _gridElements.Add(cell);

                    if (cellData.passengers != null && cellData.passengers.Length > 0)
                    {
                        var colorType = (ColorType)cellData.passengers[0].color;
                        CreatePassenger(cell, colorType);
                    }
                }
            }

            CreateTunnels(data);
            CreateExitCells(data.gridWidth, data.gridHeight, _origin);
            InitializePathfinding();
        }

        public void Cleanup()
        {
            for (int i = 0; i < _gridElements.Count; i++)
            {
                if (_gridElements[i] == null)
                {
                    continue;
                }
                if (_gridElements[i] is Cell cell)
                {
                    gridFactory.ReturnCell(cell);
                }
                else if (_gridElements[i] is Tunnel tunnel)
                {
                    gridFactory.ReturnTunnel(tunnel);
                }
            }
            for (int i = 0; i < _exitPointsList.Count; i++)
            {
                gridFactory.ReturnExitPoint(_exitPointsList[i]);
            }
            _gridElements.Clear();
            _exitPointsList.Clear();
            _tunnelsList.Clear();
        }

        public void CreatePassenger(Cell cell, ColorType color)
        {
            var passenger = passengerFactory.Get(color);
            passenger.transform.position = cell.GetWorldPosition();
            passenger.transform.rotation = Quaternion.identity;
            cell.SetPassenger(passenger);
            cell.Arrive();
        }

        public List<Vector3> FindPath(Cell cell)
        {
            var pathNodes = _aStar.FindPathToNearest(cell, _exitTargetsList);
            if (pathNodes == null)
            {
                return null;
            }
            var positionsList = new List<Vector3>();
            for (int i = 0; i < pathNodes.Count; i++)
            {
                positionsList.Add(pathNodes[i].GetWorldPosition());
            }
            return positionsList;
        }

        public IGridElement GetElementAt(Vector2Int gridPosition)
        {
            int index = gridPosition.y * _gridWidth + gridPosition.x;
            if (index < 0 || index >= _gridElements.Count)
            {
                return null;
            }
            return _gridElements[index];
        }

        public Cell GetCellAt(Vector2Int gridPosition)
        {
            return GetElementAt(gridPosition) as Cell;
        }

        public void UpdateMovableStates()
        {
            for (int i = 0; i < _gridElements.Count; i++)
            {
                Cell cell = _gridElements[i] as Cell;
                if (cell == null)
                {
                    continue;
                }
                if (cell.Passenger == null)
                {
                    cell.SetMovable(false, null);
                    continue;
                }
                List<Vector3> path = FindPath(cell);
                cell.SetMovable(path != null, path);
            }
        }

        public void CheckTunnels()
        {
            for (int i = 0; i < _tunnelsList.Count; i++)
            {
                var tunnel = _tunnelsList[i];
                if (tunnel.CanSpawn())
                {
                    var color = tunnel.SpawnNext();
                    SpawnFromTunnel(tunnel, color);
                }
            }
        }

        private void CreateExitCells(int width, int height, Vector3 origin)
        {
            for (int x = 0; x < width; x++)
            {
                var position = origin + new Vector3(x * CELL_SPACING, 0f, CELL_SPACING);
                var exitPoint = gridFactory.GetExitPoint(new Vector2Int(x, -1));
                exitPoint.transform.position = position;
                exitPoint.transform.rotation = Quaternion.identity;
                _exitPointsList.Add(exitPoint);
            }
        }

        private void InitializePathfinding()
        {
            var allNodes = new List<IGridElement>();
            _exitTargetsList = new List<IGridElement>();
            for (int i = 0; i < _gridElements.Count; i++)
            {
                if (_gridElements[i] == null)
                {
                    continue;
                }
                allNodes.Add(_gridElements[i]);
            }
            for (int i = 0; i < _exitPointsList.Count; i++)
            {
                allNodes.Add(_exitPointsList[i]);
                _exitTargetsList.Add(_exitPointsList[i]);
            }
            _aStar.SetNodes(allNodes);
        }

        private Vector3 CalculateGridOrigin(int width)
        {
            var offsetX = (width - 1) * CELL_SPACING * 0.5f;
            return new Vector3(-offsetX, 0f, 0f);
        }

        private void CreateTunnels(LevelData data)
        {
            for (int y = 0; y < data.gridHeight; y++)
            {
                for (int x = 0; x < data.gridWidth; x++)
                {
                    var cellData = data.rows[y].cells[x];
                    if (!cellData.isTunnel)
                    {
                        continue;
                    }
                    var offset = DIRECTION_OFFSETS[cellData.orientation];
                    var targetPos = new Vector2Int(x + offset.x, y + offset.y);
                    var targetCell = GetCellAt(targetPos);
                    if (targetCell == null)
                    {
                        continue;
                    }
                    var gridPos = new Vector2Int(x, y);
                    var position = _origin + new Vector3(x * CELL_SPACING, 0f, -y * CELL_SPACING);
                    var tunnel = gridFactory.GetTunnel(cellData, targetCell);
                    tunnel.GridPosition = gridPos;
                    tunnel.transform.position = position;
                    tunnel.transform.rotation = Quaternion.identity;
                    int index = y * _gridWidth + x;
                    _gridElements[index] = tunnel;
                    _tunnelsList.Add(tunnel);
                }
            }
        }

        private void SpawnFromTunnel(Tunnel tunnel, ColorType color)
        {
            var targetCell = tunnel.TargetCell;
            var passenger = passengerFactory.Get(color);
            passenger.transform.position = tunnel.transform.position;
            passenger.transform.rotation = Quaternion.identity;
            targetCell.SetPassenger(passenger);
            passenger.MoveToCell(targetCell.transform, () =>
            {
                targetCell.Arrive();
            });
        }
    }
}
