using BusJam.Pooling;
using UnityEngine;

namespace BusJam.WaitingSlot
{
    public class WaitingSlotFactory : MonoBehaviour
    {
        [SerializeField]
        private WaitingSlot prefab;

        [SerializeField]
        private Transform poolParent;

        private ObjectPool<WaitingSlot> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<WaitingSlot>(prefab, poolParent);
        }

        public WaitingSlot Get()
        {
            var cell = _pool.Get();
            cell.Clear();
            return cell;
        }

        public void Return(WaitingSlot slot)
        {
            slot.Clear();
            _pool.Return(slot);
        }
    }
}
