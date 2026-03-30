using System;

namespace BusJam.Data
{
    [Serializable]
    public class LevelData
    {
        public int id;
        public int gridWidth;
        public int gridHeight;
        public RowData[] rows;
        public BusData[] buses;
        public int timeLimit;
        public int waitingCellCount;
        public int cameraPreset;
    }

    [Serializable]
    public class RowData
    {
        public CellData[] cells;
    }

    [Serializable]
    public class CellData
    {
        public PassengerData[] passengers;
        public bool isBlocked;
        public bool isTunnel;
        public int orientation;
    }

    [Serializable]
    public class PassengerData
    {
        public int color;
    }

    [Serializable]
    public class BusData
    {
        public int color;
        public int capacity;
    }
}