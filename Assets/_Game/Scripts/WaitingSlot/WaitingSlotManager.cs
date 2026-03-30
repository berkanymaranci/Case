using System.Collections.Generic;
using BusJam.Data;
using UnityEngine;

namespace BusJam.WaitingSlot
{
    public class WaitingSlotManager : MonoBehaviour
    {
        [SerializeField]
        private WaitingSlotFactory waitingSlotFactory;

        [SerializeField]
        private Transform waitingParent;

        [SerializeField]
        private float cellSpacing = 1f;

        private List<WaitingSlot> _waitingCellsList;

        public bool IsFull
        {
            get
            {
                for (int i = 0; i < _waitingCellsList.Count; i++)
                {
                    if (_waitingCellsList[i].IsEmpty)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void CreateCells(int count)
        {
            _waitingCellsList = new List<WaitingSlot>();
            var startX = -((count - 1) * cellSpacing * 0.5f);
            for (int i = 0; i < count; i++)
            {
                var position = new Vector3(startX + i * cellSpacing, 0f, 0f);
                var cell = waitingSlotFactory.Get();
                cell.transform.SetParent(waitingParent);
                cell.transform.localPosition = position;
                _waitingCellsList.Add(cell);
            }
        }

        public void Cleanup()
        {
            for (int i = 0; i < _waitingCellsList.Count; i++)
            {
                waitingSlotFactory.Return(_waitingCellsList[i]);
            }
            _waitingCellsList.Clear();
        }

        public WaitingSlot GetEmptyCell()
        {
            for (int i = 0; i < _waitingCellsList.Count; i++)
            {
                if (_waitingCellsList[i].IsEmpty)
                {
                    return _waitingCellsList[i];
                }
            }
            return null;
        }

        public WaitingSlot GetMatchingCell(ColorType busColor)
        {
            for (int i = 0; i < _waitingCellsList.Count; i++)
            {
                var cell = _waitingCellsList[i];
                if (!cell.IsArrived)
                {
                    continue;
                }
                if (cell.Passenger.Color != busColor)
                {
                    continue;
                }
                return cell;
            }
            return null;
        }
    }
}