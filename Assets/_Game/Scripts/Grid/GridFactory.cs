using BusJam.Data;
using BusJam.Pooling;
using UnityEngine;

namespace BusJam.Grid
{
    public class GridFactory : MonoBehaviour
    {
        [SerializeField]
        private Cell cellPrefab;

        [SerializeField]
        private ExitPoint exitPointPrefab;

        [SerializeField]
        private Tunnel tunnelPrefab;

        [SerializeField]
        private Transform poolParent;

        private ObjectPool<Cell> _cellPool;
        private ObjectPool<ExitPoint> _exitPointPool;
        private ObjectPool<Tunnel> _tunnelPool;

        private void Awake()
        {
            _cellPool = new ObjectPool<Cell>(cellPrefab, poolParent);
            _exitPointPool = new ObjectPool<ExitPoint>(exitPointPrefab, poolParent);
            _tunnelPool = new ObjectPool<Tunnel>(tunnelPrefab, poolParent);
        }

        public Cell GetCell(Vector2Int gridPosition, bool isBlocked)
        {
            var cell = _cellPool.Get();
            cell.Init(gridPosition, isBlocked);
            return cell;
        }

        public void ReturnCell(Cell cell)
        {
            _cellPool.Return(cell);
        }

        public ExitPoint GetExitPoint(Vector2Int gridPosition)
        {
            var exitPoint = _exitPointPool.Get();
            exitPoint.GridPosition = gridPosition;
            return exitPoint;
        }

        public void ReturnExitPoint(ExitPoint exitPoint)
        {
            _exitPointPool.Return(exitPoint);
        }

        public Tunnel GetTunnel(CellData cellData, Cell targetCell)
        {
            var tunnel = _tunnelPool.Get();
            tunnel.Init(cellData, targetCell);
            return tunnel;
        }

        public void ReturnTunnel(Tunnel tunnel)
        {
            _tunnelPool.Return(tunnel);
        }
    }
}