using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EnemyCharacter : MonoBehaviour
{
    [Header("Character Configuration")]
    [SerializeField] private CharacterStats characterStats;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private int wallCollisionDamage = 5;
    
    private CharacterModel model;
    private bool isDying = false;
    private bool isKnockingBack = false;
    
    public CharacterStats Stats => characterStats;
    public CharacterModel Model => model;
    
    public event Action<EnemyCharacter> OnEnemyDied;
    
    private void Awake()
    {
        InitializeModel();
    }
    
    private void InitializeModel()
    {
        if (characterStats != null)
        {
            model = new CharacterModel(characterStats);
            Debug.Log($"[EnemyCharacter] {gameObject.name} model initialized - HP: {model.CurrentHp}/{model.MaxHp}");
        }
        else
        {
            Debug.LogWarning($"[EnemyCharacter] {gameObject.name} has no CharacterStats assigned!");
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (model != null && !isDying)
        {
            model.TakeDamage(damage);
            Debug.Log($"[EnemyCharacter] {gameObject.name} took {damage} damage. HP: {model.CurrentHp}/{model.MaxHp}");
            
            if (model.IsDead())
            {
                Die();
            }
        }
    }
    
    private void Die()
    {
        if (isDying) return;
        
        isDying = true;
        Debug.Log($"[EnemyCharacter] {gameObject.name} has died!");
        
        // Fire death event
        OnEnemyDied?.Invoke(this);
        
        // Get grid position before destroying
        Vector2Int gridPos = GetGridPosition();
        
        // Clear from grid
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearGridPosition(gridPos);
        }
        
        // Destroy with delay
        Destroy(gameObject, 0.25f);
    }
    
    private Vector2Int GetGridPosition()
    {
        if (GridManager.Instance != null)
        {
            var cell = GridManager.Instance.GetCellFromWorldPosition(transform.position);
            if (cell != null)
            {
                return cell.GridPosition;
            }
        }
        return new Vector2Int(-1, -1);
    }
    
    public IEnumerator ProcessTurn()
    {
        if (isDying || isKnockingBack) yield break;
        
        Debug.Log($"[EnemyCharacter] {gameObject.name} is processing turn...");
        
        // Try to move using EnemyMovement component
        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null)
        {
            yield return movement.ProcessMovement();
        }
        else
        {
            Debug.LogWarning($"[EnemyCharacter] {gameObject.name} has no EnemyMovement component");
            yield return new WaitForSeconds(0.5f); // Fallback delay
        }
        
        Debug.Log($"[EnemyCharacter] {gameObject.name} turn complete");
    }
    
    public IEnumerator ApplyKnockback(Direction knockbackDirection)
    {
        if (isDying || isKnockingBack) yield break;
        
        isKnockingBack = true;
        Debug.Log($"[EnemyCharacter] {gameObject.name} knockback in {knockbackDirection} direction!");
        
        Vector2Int currentPos = GetGridPosition();
        Vector2Int knockbackOffset = GetDirectionVector(knockbackDirection);
        Vector2Int targetPos = currentPos + knockbackOffset;
        
        // Check if target position is valid and empty
        bool canKnockback = false;
        bool willHitWall = false;
        
        if (GridManager.Instance.IsValidPosition(targetPos.x, targetPos.y))
        {
            GridCell targetCell = GridManager.Instance.GetCell(targetPos.x, targetPos.y);
            if (!targetCell.IsOccupied && !targetCell.IsObstacle)
            {
                canKnockback = true;
            }
            else
            {
                willHitWall = true;
                Debug.Log($"[EnemyCharacter] Knockback blocked by obstacle/occupied cell at {targetPos}");
            }
        }
        else
        {
            willHitWall = true;
            Debug.Log($"[EnemyCharacter] Knockback blocked by map edge");
        }
        
        if (canKnockback)
        {
            // Clear current position
            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.ClearGridPosition(currentPos);
            }
            
            // Move to target position
            Vector3 targetWorldPos = GridManager.Instance.GetCellWorldPosition(targetPos.x, targetPos.y);
            
            yield return transform.DOMove(targetWorldPos, knockbackDuration)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();
            
            // Register at new position
            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.RegisterObject(gameObject, targetPos);
            }
            
            // Update EnemyMovement grid position if exists
            EnemyMovement movement = GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.UpdateGridPosition(targetPos);
            }
            
            Debug.Log($"[EnemyCharacter] Knockback complete, moved to {targetPos}");
        }
        else if (willHitWall)
        {
            // Take extra damage for hitting wall/obstacle
            Debug.Log($"[EnemyCharacter] {gameObject.name} hits wall! Taking {wallCollisionDamage} extra damage");
            TakeDamage(wallCollisionDamage);
            
            // Small shake animation to show impact
            transform.DOShakePosition(0.2f, 0.1f, 10, 90f);
            yield return new WaitForSeconds(0.2f);
        }
        
        isKnockingBack = false;
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
}