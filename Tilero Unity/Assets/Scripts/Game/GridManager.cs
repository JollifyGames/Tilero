using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour, IManager
{
    public static GridManager Instance { get; private set; }
    
    [Header("Grid Settings")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(9, 9);
    [SerializeField] private Vector3 gridWorldPosition = Vector3.zero;
    [SerializeField] private float cellSize = 1f;
    
    [Header("Test Settings")]
    [SerializeField] private GameObject testPrefab;
    [SerializeField] private Transform cellParent;
    
    [Header("Preview Settings")]
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private Transform previewParent;
    
    private GridCell[,] grid;
    private System.Collections.Generic.List<GameObject> activePreviewObjects = new System.Collections.Generic.List<GameObject>();
    
    public Vector2Int GridSize => gridSize;
    public Vector3 GridWorldPosition => gridWorldPosition;
    public float CellSize => cellSize;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public IEnumerator Initialize()
    {
        CreateGrid();
        yield return null;
    }
    
    private void CreateGrid()
    {
        grid = new GridCell[gridSize.x, gridSize.y];
        
        if (cellParent == null)
        {
            GameObject parentObject = new GameObject("GridCells");
            parentObject.transform.position = gridWorldPosition;
            cellParent = parentObject.transform;
        }
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 cellWorldPos = GetCellWorldPosition(x, y);
                grid[x, y] = new GridCell(x, y, cellWorldPos);
                
                if (testPrefab != null)
                {
                    GameObject cellObject = Instantiate(testPrefab, cellWorldPos, Quaternion.identity, cellParent);
                    cellObject.name = $"Cell_{x}_{y}";
                }
            }
        }
        
        Debug.Log($"[GridManager] Grid created: {gridSize.x}x{gridSize.y} at position {gridWorldPosition}");
    }
    
    public Vector3 GetCellWorldPosition(int x, int y)
    {
        float gridWidth = gridSize.x * cellSize;
        float gridHeight = gridSize.y * cellSize;
        
        float xPos = gridWorldPosition.x - (gridWidth * 0.5f) + (x * cellSize) + (cellSize * 0.5f);
        float yPos = gridWorldPosition.y - (gridHeight * 0.5f) + (y * cellSize) + (cellSize * 0.5f);
        float zPos = gridWorldPosition.z;
        
        return new Vector3(xPos, yPos, zPos);
    }
    
    public GridCell GetCell(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            return grid[x, y];
        }
        return null;
    }
    
    public GridCell GetCellFromWorldPosition(Vector3 worldPosition)
    {
        float gridWidth = gridSize.x * cellSize;
        float gridHeight = gridSize.y * cellSize;
        
        float offsetX = worldPosition.x - (gridWorldPosition.x - gridWidth * 0.5f);
        float offsetY = worldPosition.y - (gridWorldPosition.y - gridHeight * 0.5f);
        
        int x = Mathf.FloorToInt(offsetX / cellSize);
        int y = Mathf.FloorToInt(offsetY / cellSize);
        
        return GetCell(x, y);
    }
    
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
    }
    
    public GridCell[,] GetGrid()
    {
        return grid;
    }
    
    public void ShowPatternPreview(System.Collections.Generic.List<Vector2Int> positions)
    {
        ClearPatternPreview();
        
        if (previewPrefab == null)
        {
            Debug.LogWarning("[GridManager] Preview prefab not assigned!");
            return;
        }
        
        if (previewParent == null)
        {
            GameObject parentObject = new GameObject("PreviewObjects");
            parentObject.transform.position = gridWorldPosition;
            previewParent = parentObject.transform;
        }
        
        foreach (var pos in positions)
        {
            if (IsValidPosition(pos.x, pos.y))
            {
                Vector3 worldPos = GetCellWorldPosition(pos.x, pos.y);
                GameObject preview = Instantiate(previewPrefab, worldPos, Quaternion.identity, previewParent);
                preview.name = $"Preview_{pos.x}_{pos.y}";
                activePreviewObjects.Add(preview);
            }
        }
        
        Debug.Log($"[GridManager] Showing preview for {activePreviewObjects.Count} cells");
    }
    
    public void ClearPatternPreview()
    {
        foreach (var preview in activePreviewObjects)
        {
            if (preview != null)
                Destroy(preview);
        }
        activePreviewObjects.Clear();
        
        Debug.Log("[GridManager] Pattern preview cleared");
    }
}