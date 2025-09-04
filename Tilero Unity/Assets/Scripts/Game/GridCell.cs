using UnityEngine;

[System.Serializable]
public class GridCell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool IsOccupied { get; set; }
    public GameObject OccupyingObject { get; set; }
    public GameObject OccupiedObject => OccupyingObject; // Alias for compatibility
    public Vector2Int GridPosition => new Vector2Int(X, Y);
    
    public GridCell(int x, int y, Vector3 worldPosition)
    {
        X = x;
        Y = y;
        WorldPosition = worldPosition;
        IsOccupied = false;
        OccupyingObject = null;
    }
    
    public void SetOccupied(GameObject occupyingObject)
    {
        IsOccupied = true;
        OccupyingObject = occupyingObject;
    }
    
    public void ClearOccupation()
    {
        IsOccupied = false;
        OccupyingObject = null;
    }
}