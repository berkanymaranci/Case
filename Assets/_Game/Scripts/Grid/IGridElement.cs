using UnityEngine;

namespace BusJam.Grid
{
    public interface IGridElement
    {
        Vector2Int GridPosition { get; set; }
        bool IsWalkable { get; }
        Vector3 GetWorldPosition(); 
    }
}
