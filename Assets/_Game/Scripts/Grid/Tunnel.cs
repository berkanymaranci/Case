using System.Collections.Generic;
using BusJam.Data;
using TMPro;
using UnityEngine;

namespace BusJam.Grid
{
    public class Tunnel : MonoBehaviour, IGridElement
    {
        [SerializeField]
        private Transform model;

        [SerializeField]
        private TextMeshPro countText;

        private Vector2Int _gridPosition;
        private List<PassengerData> _passengerQueue;
        private Cell _targetCell;
        private int _orientation;

        public Cell TargetCell => _targetCell;
        public bool IsWalkable => false;
        public bool HasPassengers => _passengerQueue.Count > 0;

        public Vector2Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        public void Init(CellData cellData, Cell targetCell)
        {
            _orientation = cellData.orientation;
            _targetCell = targetCell;
            _passengerQueue = new List<PassengerData>();
            if (cellData.passengers != null)
            {
                for (int i = 0; i < cellData.passengers.Length; i++)
                {
                    _passengerQueue.Add(cellData.passengers[i]);
                }
            }
            float yRotation = _orientation * 90f;
            model.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            UpdateCountText();
        }

        public bool CanSpawn()
        {
            if (_passengerQueue.Count == 0)
            {
                return false;
            }
            if (_targetCell.Passenger != null)
            {
                return false;
            }
            return true;
        }

        public ColorType SpawnNext()
        {
            PassengerData next = _passengerQueue[0];
            _passengerQueue.RemoveAt(0);
            UpdateCountText();
            return (ColorType)next.color;
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }
        
        
        private void UpdateCountText()
        {
            if (countText == null)
            {
                return;
            }
            countText.text = _passengerQueue.Count.ToString();
        }
    }
}
