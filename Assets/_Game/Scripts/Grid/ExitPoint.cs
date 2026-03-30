using UnityEngine;

namespace BusJam.Grid
{
    public class ExitPoint : MonoBehaviour, IGridElement
    {
        private Vector2Int _gridPosition;

        public Vector2Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        public bool IsWalkable => true;

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }
    }
}