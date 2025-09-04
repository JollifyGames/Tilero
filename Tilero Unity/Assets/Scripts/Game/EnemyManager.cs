using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IManager
{
    public static EnemyManager Instance { get; private set; }
    
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public Vector2Int gridPosition;
    }
    
    [Header("Enemy Configuration")]
    [SerializeField] private List<EnemySpawnInfo> enemySpawnList = new List<EnemySpawnInfo>();
    
    private List<EnemyCharacter> activeEnemies = new List<EnemyCharacter>();
    
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
        SpawnEnemies();
        yield return null;
    }
    
    private void SpawnEnemies()
    {
        if (GridManager.Instance == null || BoardManager.Instance == null)
        {
            Debug.LogError("[EnemyManager] GridManager or BoardManager not found!");
            return;
        }
        
        foreach (var spawnInfo in enemySpawnList)
        {
            if (spawnInfo.enemyPrefab == null)
            {
                Debug.LogWarning("[EnemyManager] Enemy prefab is null, skipping...");
                continue;
            }
            
            if (!GridManager.Instance.IsValidPosition(spawnInfo.gridPosition.x, spawnInfo.gridPosition.y))
            {
                Debug.LogWarning($"[EnemyManager] Invalid grid position {spawnInfo.gridPosition}, skipping...");
                continue;
            }
            
            Vector3 worldPosition = GridManager.Instance.GetCellWorldPosition(spawnInfo.gridPosition.x, spawnInfo.gridPosition.y);
            GameObject enemyGO = Instantiate(spawnInfo.enemyPrefab, worldPosition, Quaternion.identity);
            enemyGO.name = $"Enemy_{spawnInfo.gridPosition.x}_{spawnInfo.gridPosition.y}";
            
            EnemyCharacter enemyCharacter = enemyGO.GetComponent<EnemyCharacter>();
            if (enemyCharacter != null)
            {
                activeEnemies.Add(enemyCharacter);
                BoardManager.Instance.RegisterObject(enemyGO, spawnInfo.gridPosition);
                
                // Subscribe to death event
                enemyCharacter.OnEnemyDied += HandleEnemyDeath;
                
                // Update EnemyMovement grid position if exists
                EnemyMovement movement = enemyGO.GetComponent<EnemyMovement>();
                if (movement != null)
                {
                    movement.UpdateGridPosition(spawnInfo.gridPosition);
                }
                
                Debug.Log($"[EnemyManager] Spawned enemy at grid position {spawnInfo.gridPosition}");
            }
            else
            {
                Debug.LogError($"[EnemyManager] Enemy prefab does not have EnemyCharacter component!");
                Destroy(enemyGO);
            }
        }
        
        Debug.Log($"[EnemyManager] Spawned {activeEnemies.Count} enemies");
    }
    
    public List<EnemyCharacter> GetActiveEnemies()
    {
        return new List<EnemyCharacter>(activeEnemies);
    }
    
    public void RemoveEnemy(EnemyCharacter enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            Debug.Log($"[EnemyManager] Enemy removed. Active enemies: {activeEnemies.Count}");
        }
    }
    
    public IEnumerator ProcessAllEnemyTurns()
    {
        Debug.Log($"[EnemyManager] Processing turns for {activeEnemies.Count} enemies");
        
        // Use a copy to avoid collection modification during iteration
        List<EnemyCharacter> enemiesCopy = new List<EnemyCharacter>(activeEnemies);
        
        foreach (var enemy in enemiesCopy)
        {
            if (enemy != null && activeEnemies.Contains(enemy))
            {
                yield return enemy.ProcessTurn();
            }
        }
        
        Debug.Log("[EnemyManager] All enemy turns processed");
    }
    
    private void HandleEnemyDeath(EnemyCharacter enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            // Unsubscribe from event
            enemy.OnEnemyDied -= HandleEnemyDeath;
            
            // Remove from active list
            activeEnemies.Remove(enemy);
            
            Debug.Log($"[EnemyManager] Enemy died and removed from list. Active enemies: {activeEnemies.Count}");
            
            // Check win condition
            if (activeEnemies.Count == 0)
            {
                Debug.Log("[EnemyManager] === ALL ENEMIES DEFEATED! ===");
                // Win condition can be handled later
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from all enemy events
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDied -= HandleEnemyDeath;
            }
        }
    }
}