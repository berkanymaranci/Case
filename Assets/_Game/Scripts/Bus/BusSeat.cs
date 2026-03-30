using BusJam.Passengers;
using UnityEngine;

namespace BusJam.Bus
{
    
    public class BusSeat: MonoBehaviour
    {
        private Passenger _passenger;
        private bool _isArrived;

        public Passenger Passenger => _passenger;
        public bool IsEmpty => _passenger == null;
        public bool HasPassenger => _passenger != null;
        public bool IsArrived => _isArrived;

        public void SetPassenger(Passenger passenger)
        {
            _passenger = passenger;
            _isArrived = false;
        }

        public void Arrive()
        {
            _isArrived = true;
        }

        public void Clear()
        {
            _passenger = null;
            _isArrived = false;
        }
    }
}