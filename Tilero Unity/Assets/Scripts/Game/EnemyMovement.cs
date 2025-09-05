using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private int movementRange = 1;
    [SerializeField] private float moveAnimationDuration = 0.3f;
    
    [Header("Visual Settings")]
    [SerializeField] private Transform visual;
    [SerializeField] private Animator animator;
    
    private Direction currentDirection = Direction.Down;
    private bool isMoving = false;
    private Vector2Int currentGridPosition;
    
    public Direction CurrentDirection => currentDirection;
    public bool IsMoving => isMoving;
    
    private void Awake()
    {
        if (visual == null)
            visual = transform.GetChild(0);
        
        if (animator == null && visual != null)
            animator = visual.GetComponent<Animator>();
        
        // Get initial grid position
        if (GridManager.Instance != null)
        {
            var cell = GridManager.Instance.GetCellFromWorldPosition(transform.position);
            if (cell != null)
            {
                currentGridPosition = cell.GridPosition;
            }
        }
    }
    
    public IEnumerator ProcessMovement()
    {
        if (isMoving) yield break;
        
        // Check for player in adjacent cells
        bool playerFound = false;
        Vector2Int playerPosition = Vector2Int.zero;
        
        // Check all 4 directions for player
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (var dir in directions)
        {
            Vector2Int checkPos = currentGridPosition + dir;
            if (IsPlayerAt(checkPos))
            {
                playerFound = true;
                playerPosition = checkPos;
                Debug.Log($"[EnemyMovement] {gameObject.name} found player at {checkPos}");
                break;
            }
        }
        
        // If player not found, try to move randomly
        if (!playerFound)
        {
            yield return MoveRandomly();
        }
        else
        {
            // Face the player but don't move (player is adjacent)
            Direction dirToPlayer = GetDirectionTo(playerPosition);
            SetDirection(dirToPlayer);
            Debug.Log($"[EnemyMovement] {gameObject.name} facing player, ready to attack");
        }
    }
    
    private IEnumerator MoveRandomly()
    {
        List<Vector2Int> validMoves = GetValidMovePositions();
        
        if (validMoves.Count == 0)
        {
            Debug.Log($"[EnemyMovement] {gameObject.name} has no valid moves");
            yield break;
        }
        
        // Choose random valid position
        Vector2Int targetPosition = validMoves[Random.Range(0, validMoves.Count)];
        
        // Update direction
        Direction moveDirection = GetDirectionTo(targetPosition);
        SetDirection(moveDirection);
        
        // Move to target position
        yield return MoveToPosition(targetPosition);
    }
    
    private List<Vector2Int> GetValidMovePositions()
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();
        
        // Check all positions within movement range
        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                // Skip current position and diagonal moves
                if ((x == 0 && y == 0) || (x != 0 && y != 0))
                    continue;
                
                // Only allow moves within movement range
                if (Mathf.Abs(x) + Mathf.Abs(y) > movementRange)
                    continue;
                
                Vector2Int targetPos = currentGridPosition + new Vector2Int(x, y);
                
                if (IsValidMovePosition(targetPos))
                {
                    validPositions.Add(targetPos);
                }
            }
        }
        
        return validPositions;
    }
    
    private bool IsValidMovePosition(Vector2Int position)
    {
        if (!GridManager.Instance.IsValidPosition(position.x, position.y))
            return false;
        
        GridCell cell = GridManager.Instance.GetCell(position.x, position.y);
        return cell != null && !cell.IsOccupied && !cell.IsObstacle;
    }
    
    private bool IsPlayerAt(Vector2Int position)
    {
        if (!GridManager.Instance.IsValidPosition(position.x, position.y))
            return false;
        
        GridCell cell = GridManager.Instance.GetCell(position.x, position.y);
        if (cell != null && cell.IsOccupied && cell.OccupiedObject != null)
        {
            return cell.OccupiedObject.GetComponent<PlayerController>() != null;
        }
        
        return false;
    }
    
    private IEnumerator MoveToPosition(Vector2Int targetPosition)
    {
        if (isMoving) yield break;
        
        isMoving = true;
        
        // Clear current position
        GridCell currentCell = GridManager.Instance.GetCell(currentGridPosition.x, currentGridPosition.y);
        if (currentCell != null)
        {
            currentCell.ClearOccupation();
        }
        
        // Get target world position
        Vector3 targetWorldPos = GridManager.Instance.GetCellWorldPosition(targetPosition.x, targetPosition.y);
        
        // Trigger movement animation
        TriggerMovementAnimation();
        SetWalkingState(true);
        
        // Animate movement
        Tween moveTween = transform.DOMove(targetWorldPos, moveAnimationDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isMoving = false;
                SetWalkingState(false);
                
                // Register at new position
                GridCell targetCell = GridManager.Instance.GetCell(targetPosition.x, targetPosition.y);
                if (targetCell != null)
                {
                    targetCell.SetOccupied(gameObject);
                }
                
                currentGridPosition = targetPosition;
                Debug.Log($"[EnemyMovement] {gameObject.name} moved to {targetPosition}");
            });
        
        yield return moveTween.WaitForCompletion();
    }
    
    private Direction GetDirectionTo(Vector2Int targetPosition)
    {
        Vector2Int diff = targetPosition - currentGridPosition;
        
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            return diff.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return diff.y > 0 ? Direction.Up : Direction.Down;
        }
    }
    
    private void SetDirection(Direction direction)
    {
        currentDirection = direction;
        UpdateVisualFlip();
    }
    
    private void UpdateVisualFlip()
    {
        if (visual == null) return;
        
        Vector3 scale = visual.localScale;
        if (currentDirection == Direction.Left)
            scale.x = -Mathf.Abs(scale.x);
        else if (currentDirection == Direction.Right)
            scale.x = Mathf.Abs(scale.x);
        
        visual.localScale = scale;
    }
    
    private void TriggerMovementAnimation()
    {
        if (animator == null) return;
        
        if (currentDirection == Direction.Up)
            animator.SetTrigger("RunBack");
        else
            animator.SetTrigger("Run");
    }
    
    private void SetWalkingState(bool isWalking)
    {
        if (animator == null) return;
        animator.SetBool("IsWalking", isWalking);
    }
    
    public void UpdateGridPosition(Vector2Int newPosition)
    {
        currentGridPosition = newPosition;
    }
}