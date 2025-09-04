using System;
using System.Collections;
using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    [Header("Character Configuration")]
    [SerializeField] private CharacterStats characterStats;
    
    private CharacterModel model;
    private bool isDying = false;
    
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
        if (isDying) yield break;
        
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
}