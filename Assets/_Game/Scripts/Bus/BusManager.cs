using System;
using BusJam.Data;
using UnityEngine;

namespace BusJam.Bus
{
    public class BusManager : MonoBehaviour
    {
        [SerializeField]
        private BusFactory busFactory;

        [SerializeField]
        private Transform activePosition;

        [SerializeField]
        private Transform queuePosition;

        [SerializeField]
        private Transform exitPosition;

        [SerializeField]
        private Transform spawnPosition;

        private BusData[] _busDataArray;
        private int _currentIndex;
        private Bus _activeBus;
        private Bus _queueBus;

        public event Action OnBusChanged;

        public Bus ActiveBus => _activeBus;

        public void CreateBuses(BusData[] busDataArray)
        {
            _busDataArray = busDataArray;
            _currentIndex = 0;
            var firstBus = SpawnBus(_currentIndex);
            firstBus.transform.position = queuePosition.position;
            _activeBus = firstBus;
            firstBus.MoveTo(activePosition.position, () =>
            {
                _activeBus.Arrive();
                OnBusChanged?.Invoke();
            });
            SpawnQueueBus();
        }

        public void NextBus()
        {
            var departingBus = _activeBus;
            _currentIndex++;
            departingBus.MoveTo(exitPosition.position, () =>
            {
                busFactory.Return(departingBus);
            });
            if (_queueBus != null)
            {
                var nextBus = _queueBus;
                _queueBus = null;
                _activeBus = nextBus;
                nextBus.MoveTo(activePosition.position, () =>
                {
                    nextBus.Arrive();
                    OnBusChanged?.Invoke();
                });
                SpawnQueueBus();
            }
            else
            {
                _activeBus = null;
                OnBusChanged?.Invoke();
            }
        }

        public void Cleanup()
        {
            _activeBus = null;
            _queueBus = null;
        }

        private void SpawnQueueBus()
        {
            int queueIndex = _currentIndex + 1;
            if (queueIndex < _busDataArray.Length)
            {
                _queueBus = SpawnBus(queueIndex);
                _queueBus.MoveTo(queuePosition.position);
            }
        }

        private Bus SpawnBus(int index)
        {
            var colorType = (ColorType)_busDataArray[index].color;
            int capacity = _busDataArray[index].capacity;
            var bus = busFactory.Get(colorType, capacity);
            bus.transform.position = spawnPosition.position;
            bus.transform.rotation = spawnPosition.rotation;
            return bus;
        }
    }
}