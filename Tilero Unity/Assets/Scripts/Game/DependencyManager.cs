using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DependencyManager : MonoBehaviour
{
    public static DependencyManager Instance { get; private set; }
    
    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private WorldManager worldManager;
    
    private List<IManager> managers = new List<IManager>();
    private int initializedManagerCount = 0;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        CollectManagers();
        InitializeManagers();
    }
    
    private void CollectManagers()
    {
        // Initialize in correct dependency order
        // 1. GridManager (no dependencies)
        if (gridManager != null)
            managers.Add(gridManager);
        
        // 2. BoardManager (depends on GridManager)
        if (boardManager != null)
            managers.Add(boardManager);
        
        // 3. SlotManager (depends on BoardManager)
        if (slotManager != null)
            managers.Add(slotManager);
        
        // 4. EnemyManager (depends on GridManager and BoardManager)
        if (enemyManager != null)
            managers.Add(enemyManager);
        
        // 5. WorldManager (depends on EnemyManager)
        if (worldManager != null)
            managers.Add(worldManager);
    }
    
    private void InitializeManagers()
    {
        if (managers.Count == 0)
        {
            Debug.LogWarning("No managers to initialize!");
            OnAllManagersInitialized();
            return;
        }
        
        // Start sequential initialization
        StartCoroutine(InitializeManagersSequentially());
    }
    
    private IEnumerator InitializeManagersSequentially()
    {
        Debug.Log("[DependencyManager] Starting sequential initialization...");
        
        foreach (var manager in managers)
        {
            yield return InitializeManager(manager);
        }
        
        OnAllManagersInitialized();
    }
    
    private IEnumerator InitializeManager(IManager manager)
    {
        Debug.Log($"[DependencyManager] Initializing {manager.GetType().Name}...");
        yield return manager.Initialize();
        
        initializedManagerCount++;
        Debug.Log($"[DependencyManager] {manager.GetType().Name} initialized ({initializedManagerCount}/{managers.Count})");
    }
    
    private void OnAllManagersInitialized()
    {
        Debug.Log("[DependencyManager] All managers initialized! Game is starting...");
    }
}