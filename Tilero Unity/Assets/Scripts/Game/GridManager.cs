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
    
    [Header("Obstacle Settings")]
    [SerializeField] private System.Collections.Generic.List<Vector2Int> obstaclesList = new System.Collections.Generic.List<Vector2Int>();
    [SerializeField] private GameObject obstaclePrefab;
    
    [Header("Border Settings")]
    [SerializeField] private System.Collections.Generic.List<Vector2Int> bordersList = new System.Collections.Generic.List<Vector2Int>();
    [SerializeField] private GameObject borderPrefab;
    
    private GridCell[,] grid;
    private System.Collections.Generic.List<GameObject> activePreviewObjects = new System.Collections.Generic.List<GameObject>();
    private System.Collections.Generic.List<GameObject> obstacleObjects = new System.Collections.Generic.List<GameObject>();
    private System.Collections.Generic.List<GameObject> borderObjects = new System.Collections.Generic.List<GameObject>();
    
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
        CreateObstacles();
        CreateBorders();
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
    
    private void CreateObstacles()
    {
        if (obstaclesList == null || obstaclesList.Count == 0)
        {
            Debug.Log("[GridManager] No obstacles to create");
            return;
        }
        
        foreach (var obstaclePos in obstaclesList)
        {
            if (IsValidPosition(obstaclePos.x, obstaclePos.y))
            {
                GridCell cell = grid[obstaclePos.x, obstaclePos.y];
                cell.IsObstacle = true;
                cell.IsOccupied = true;
                
                if (obstaclePrefab != null)
                {
                    GameObject obstacle = Instantiate(obstaclePrefab, cell.WorldPosition, Quaternion.identity, cellParent);
                    obstacle.name = $"Obstacle_{obstaclePos.x}_{obstaclePos.y}";
                    obstacleObjects.Add(obstacle);
                    cell.OccupyingObject = obstacle;
                }
                
                Debug.Log($"[GridManager] Obstacle created at position ({obstaclePos.x}, {obstaclePos.y})");
            }
            else
            {
                Debug.LogWarning($"[GridManager] Invalid obstacle position: ({obstaclePos.x}, {obstaclePos.y})");
            }
        }
        
        Debug.Log($"[GridManager] Created {obstacleObjects.Count} obstacles");
    }
    
    private void CreateBorders()
    {
        if (bordersList == null || bordersList.Count == 0)
        {
            Debug.Log("[GridManager] No borders to create");
            return;
        }
        
        foreach (var borderPos in bordersList)
        {
            if (IsValidPosition(borderPos.x, borderPos.y))
            {
                GridCell cell = grid[borderPos.x, borderPos.y];
                cell.IsBorder = true;
                // Borders are not marked as occupied, they can be knockbacked into
                
                if (borderPrefab != null)
                {
                    GameObject border = Instantiate(borderPrefab, cell.WorldPosition, Quaternion.identity, cellParent);
                    border.name = $"Border_{borderPos.x}_{borderPos.y}";
                    borderObjects.Add(border);
                }
                
                Debug.Log($"[GridManager] Border created at position ({borderPos.x}, {borderPos.y})");
            }
            else
            {
                Debug.LogWarning($"[GridManager] Invalid border position: ({borderPos.x}, {borderPos.y})");
            }
        }
        
        Debug.Log($"[GridManager] Created {borderObjects.Count} borders");
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
    
    public bool IsWalkable(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return false;
        
        GridCell cell = grid[x, y];
        return !cell.IsObstacle && !cell.IsOccupied && !cell.IsBorder;
    }
    
    public bool IsWalkable(Vector2Int position)
    {
        return IsWalkable(position.x, position.y);
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