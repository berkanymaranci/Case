using System.Collections.Generic;
using BusJam.Data;
using BusJam.Passengers;
using BusJam.Pooling;
using UnityEngine;

namespace BusJam.Bus
{
    public class BusFactory : MonoBehaviour
    {
        [SerializeField]
        private Bus[] busPrefabArray;

        [SerializeField]
        private Transform poolParent;

        [SerializeField]
        private ColorPalette colorPalette;

        [SerializeField]
        private PassengerFactory passengerFactory;

        private Dictionary<int, ObjectPool<Bus>> _poolDict;
        private List<Bus> _activeBusesList;

        private void Awake()
        {
            _poolDict = new Dictionary<int, ObjectPool<Bus>>();
            _activeBusesList = new List<Bus>();
            for (int i = 0; i < busPrefabArray.Length; i++)
            {
                _poolDict[i] = new ObjectPool<Bus>(busPrefabArray[i], poolParent);
            }
        }

        public Bus Get(ColorType colorType, int capacity)
        {
            var bus = _poolDict[capacity - 1].Get();
            bus.Init(colorType, colorPalette.GetColor(colorType));
            _activeBusesList.Add(bus);
            return bus;
        }

        public void Return(Bus bus)
        {
            if (!_activeBusesList.Remove(bus))
            {
                return;
            }
            bus.ReleasePassengers(passengerFactory.Return);
            bus.ResetState();
            _poolDict[bus.SeatCount - 1].Return(bus);
        }

        public void ReturnAll()
        {
            for (int i = _activeBusesList.Count - 1; i >= 0; i--)
            {
                var bus = _activeBusesList[i];
                bus.ReleasePassengers(passengerFactory.Return);
                bus.ResetState();
                _poolDict[bus.SeatCount - 1].Return(bus);
            }
            _activeBusesList.Clear();
        }
    }
}