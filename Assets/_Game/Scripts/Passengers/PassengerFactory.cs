using System.Collections.Generic;
using BusJam.Data;
using BusJam.Pooling;
using UnityEngine;

namespace BusJam.Passengers
{
    public class PassengerFactory : MonoBehaviour
    {
        [SerializeField]
        private Passenger prefab;

        [SerializeField]
        private Transform poolParent;

        [SerializeField]
        private ColorPalette colorPalette;

        private ObjectPool<Passenger> _pool;
        private List<Passenger> _activePassengersList;

        private void Awake()
        {
            _pool = new ObjectPool<Passenger>(prefab, poolParent);
            _activePassengersList = new List<Passenger>();
        }

        public Passenger Get(ColorType colorType)
        {
            var passenger = _pool.Get();
            passenger.Init(colorType, colorPalette.GetColor(colorType));
            _activePassengersList.Add(passenger);
            return passenger;
        }

        public void Return(Passenger passenger)
        {
            if (!_activePassengersList.Remove(passenger))
            {
                return;
            }
            passenger.ResetState();
            _pool.Return(passenger);
        }

        public void ReturnAll()
        {
            for (int i = _activePassengersList.Count - 1; i >= 0; i--)
            {
                var passenger = _activePassengersList[i];
                passenger.ResetState();
                _pool.Return(passenger);
            }
            _activePassengersList.Clear();
        }
    }
}