using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BoardManager : MonoBehaviour, IManager
{
    public static BoardManager Instance { get; private set; }
    
    [Header("Board Settings")]
    [SerializeField] private Vector2Int playerStartCell = new Vector2Int(4, 4);
    [SerializeField] private Direction playerStartDirection = Direction.Down;
    
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    
    private PlayerController playerController;
    private GameObject playerObject;
    private Vector2Int currentPlayerCell;
    private bool isMoving = false;
    
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
        yield return new WaitUntil(() => GridManager.Instance != null);
        
        SpawnPlayer();
        yield return null;
    }
    
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[BoardManager] Player prefab is not assigned!");
            return;
        }
        
        GridCell startCell = GridManager.Instance.GetCell(playerStartCell.x, playerStartCell.y);
        if (startCell == null)
        {
            Debug.LogError($"[BoardManager] Invalid start cell: {playerStartCell}");
            return;
        }
        
        playerObject = Instantiate(playerPrefab, startCell.WorldPosition, Quaternion.identity);
        playerObject.name = "Player";
        
        playerController = playerObject.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.Log("[BoardManager] Adding PlayerController component");
            playerController = playerObject.AddComponent<PlayerController>();
        }
        
        // Singleton check - use the Instance if it exists
        if (PlayerController.Instance != null && PlayerController.Instance != playerController)
        {
            playerController = PlayerController.Instance;
            Debug.Log("[BoardManager] Using existing PlayerController Instance");
        }
        
        if (playerController != null)
        {
            playerController.Initialize(this);
            playerController.SetDirection(playerStartDirection);
        }
        else
        {
            Debug.LogError("[BoardManager] PlayerController is null after setup");
        }
        currentPlayerCell = playerStartCell;
        startCell.SetOccupied(playerObject);
        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetPlayerController(playerController);
        }
        
        Debug.Log($"[BoardManager] Player spawned at cell ({playerStartCell.x}, {playerStartCell.y}) facing {playerStartDirection}");
    }
    
    public bool TryMoveObject(GameObject obj, Vector2Int fromCell, Vector2Int toCell, System.Action<Vector3> onMoveApproved = null)
    {
        if (isMoving) return false;
        
        if (!GridManager.Instance.IsValidPosition(toCell.x, toCell.y))
        {
            Debug.Log($"[BoardManager] Invalid move target: {toCell}");
            return false;
        }
        
        GridCell targetCell = GridManager.Instance.GetCell(toCell.x, toCell.y);
        if (targetCell.IsObstacle)
        {
            Debug.Log($"[BoardManager] Cell {toCell} is an obstacle");
            return false;
        }
        
        if (targetCell.IsOccupied)
        {
            Debug.Log($"[BoardManager] Cell {toCell} is occupied");
            return false;
        }
        
        GridCell sourceCell = GridManager.Instance.GetCell(fromCell.x, fromCell.y);
        
        sourceCell.ClearOccupation();
        targetCell.SetOccupied(obj);
        
        if (obj == playerObject)
        {
            currentPlayerCell = toCell;
        }
        
        onMoveApproved?.Invoke(targetCell.WorldPosition);
        
        Debug.Log($"[BoardManager] Move approved from {fromCell} to {toCell}");
        return true;
    }
    
    public bool TryMovePlayer(Vector2Int direction)
    {
        if (playerController == null || playerController.IsMoving) return false;
        
        Vector2Int targetCell = currentPlayerCell + direction;
        return TryMoveObject(playerObject, currentPlayerCell, targetCell, (targetPos) => 
        {
            playerController.MoveToPosition(targetPos);
        });
    }
    
    public void SetMovingState(bool moving)
    {
        isMoving = moving;
    }
    
    public Vector2Int GetPlayerCell()
    {
        return currentPlayerCell;
    }
    
    public void RegisterObject(GameObject obj, Vector2Int gridPosition)
    {
        if (!GridManager.Instance.IsValidPosition(gridPosition.x, gridPosition.y))
        {
            Debug.LogError($"[BoardManager] Cannot register object at invalid position: {gridPosition}");
            return;
        }
        
        GridCell cell = GridManager.Instance.GetCell(gridPosition.x, gridPosition.y);
        if (cell.IsOccupied)
        {
            Debug.LogWarning($"[BoardManager] Cell {gridPosition} is already occupied!");
            return;
        }
        
        cell.SetOccupied(obj);
        Debug.Log($"[BoardManager] Registered {obj.name} at cell {gridPosition}");
    }
    
    public EnemyCharacter FindEnemyInDirection(Vector2Int position, Direction direction)
    {
        // Önce direction'a göre ilk cell'i kontrol et
        Vector2Int checkPositions = GetDirectionVector(direction);
        Vector2Int targetCell = position + checkPositions;
        
        EnemyCharacter enemy = GetEnemyAtPosition(targetCell);
        if (enemy != null)
        {
            Debug.Log($"[BoardManager] Found enemy at {targetCell} in forward direction");
            return enemy;
        }
        
        // Yan cell'leri kontrol et (sağ ve sol)
        Vector2Int[] sidePositions = GetSidePositions(direction);
        foreach (var sideOffset in sidePositions)
        {
            targetCell = position + sideOffset;
            enemy = GetEnemyAtPosition(targetCell);
            if (enemy != null)
            {
                Debug.Log($"[BoardManager] Found enemy at {targetCell} in side direction");
                return enemy;
            }
        }
        
        return null;
    }
    
    private EnemyCharacter GetEnemyAtPosition(Vector2Int position)
    {
        if (!GridManager.Instance.IsValidPosition(position.x, position.y))
            return null;
        
        GridCell cell = GridManager.Instance.GetCell(position.x, position.y);
        if (cell.IsOccupied && cell.OccupiedObject != null)
        {
            return cell.OccupiedObject.GetComponent<EnemyCharacter>();
        }
        
        return null;
    }
    
    private Vector2Int GetDirectionVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Vector2Int.up;
            case Direction.Down: return Vector2Int.down;
            case Direction.Left: return Vector2Int.left;
            case Direction.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }
    
    private Vector2Int[] GetSidePositions(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
            case Direction.Down:
                return new Vector2Int[] { Vector2Int.left, Vector2Int.right };
            case Direction.Left:
            case Direction.Right:
                return new Vector2Int[] { Vector2Int.up, Vector2Int.down };
            default:
                return new Vector2Int[] { };
        }
    }
    
    public Direction GetDirectionToTarget(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;
        
        if (diff.x > 0) return Direction.Right;
        if (diff.x < 0) return Direction.Left;
        if (diff.y > 0) return Direction.Up;
        if (diff.y < 0) return Direction.Down;
        
        return Direction.Down; // Default
    }
    
    public void ClearGridPosition(Vector2Int gridPosition)
    {
        if (GridManager.Instance.IsValidPosition(gridPosition.x, gridPosition.y))
        {
            GridCell cell = GridManager.Instance.GetCell(gridPosition.x, gridPosition.y);
            if (cell != null)
            {
                cell.ClearOccupation();
                Debug.Log($"[BoardManager] Cleared grid position {gridPosition}");
            }
        }
    }
}