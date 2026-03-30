using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam.Data
{

    [CreateAssetMenu(menuName = "BusJam/Color Palette")]
    public class ColorPalette : ScriptableObject
    {
        [Serializable]
        public class ColorEntry
        {
            public ColorType colorType;
            public Color color;
        }

        [SerializeField]
        private List<ColorEntry> colorEntries;

        private Dictionary<ColorType, Color> _colorMap;

        private void OnEnable()
        {
            BuildMap();
        }

        public Color GetColor(ColorType colorType)
        {
            if (_colorMap == null)
            {
                BuildMap();
            }
            if (_colorMap.TryGetValue(colorType, out var color))
            {
                return color;
            }
            return Color.white;
        }

        private void BuildMap()
        {
            _colorMap = new Dictionary<ColorType, Color>();
            for (int i = 0; i < colorEntries.Count; i++)
            {
                _colorMap[colorEntries[i].colorType] = colorEntries[i].color;
            }
        }
    }
}