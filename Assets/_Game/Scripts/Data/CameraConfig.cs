using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam.Data
{
    [CreateAssetMenu(menuName = "BusJam/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [Serializable]
        public class CameraPreset
        {
            public int gridWidth;
            public int gridHeight;
            public Vector3 position;
        }

        [SerializeField]
        private List<CameraPreset> presetsList;

        [SerializeField]
        private Vector3 defaultPosition;

        public List<CameraPreset> PresetsList => presetsList;
        public Vector3 DefaultPosition
        {
            get => defaultPosition;
            set => defaultPosition = value;
        }

        public Vector3 GetPosition(int index)
        {
            if (index < 0 || index >= presetsList.Count)
            {
                return defaultPosition;
            }
            return presetsList[index].position;
        }

        public int FindPresetIndex(int width, int height)
        {
            for (int i = 0; i < presetsList.Count; i++)
            {
                if (presetsList[i].gridWidth == width
                    && presetsList[i].gridHeight == height)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
