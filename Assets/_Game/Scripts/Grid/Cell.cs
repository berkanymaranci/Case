using System.Collections.Generic;
using BusJam.Passengers;
using UnityEngine;

namespace BusJam.Grid
{
    public class Cell : MonoBehaviour, IGridElement
    {
        [SerializeField]
        private Collider cellCollider;

        private Vector2Int _gridPosition;
        private Passenger _passenger;
        private bool _isBlocked;
        private bool _isMovable;
        private List<Vector3> _cachedPathList;

        public Passenger Passenger => _passenger;
        public bool IsBlocked => _isBlocked;
        public bool IsMovable => _isMovable;
        public List<Vector3> CachedPathList => _cachedPathList;

        public Vector2Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        public bool IsWalkable => _passenger == null && !_isBlocked;

        public void Init(Vector2Int gridPosition, bool isBlocked = false)
        {
            _gridPosition = gridPosition;
            _isBlocked = isBlocked;
            _passenger = null;
            _isMovable = false;
            _cachedPathList = null;
            cellCollider.enabled = false;
        }

        public bool IsClickable()
        {
            return _passenger != null && !_isBlocked;
        }

        public void Arrive()
        {
            cellCollider.enabled = true;
        }

        public void SetPassenger(Passenger passenger)
        {
            _passenger = passenger;
        }

        public void ClearPassenger()
        {
            if (_passenger != null)
            {
                _passenger.SetHighlight(false);
            }
            _passenger = null;
            _isMovable = false;
            _cachedPathList = null;
        }

        public void SetMovable(bool isMovable, List<Vector3> path)
        {
            _isMovable = isMovable;
            _cachedPathList = path;
            if (_passenger != null)
            {
                _passenger.SetHighlight(isMovable);
            }
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }
    }
}