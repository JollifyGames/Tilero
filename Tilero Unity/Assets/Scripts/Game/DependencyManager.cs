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
        if (gridManager != null)
            managers.Add(gridManager);
        
        if (boardManager != null)
            managers.Add(boardManager);
        
        if (slotManager != null)
            managers.Add(slotManager);
        
        if (enemyManager != null)
            managers.Add(enemyManager);
        
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
        
        foreach (var manager in managers)
        {
            StartCoroutine(InitializeManager(manager));
        }
    }
    
    private IEnumerator InitializeManager(IManager manager)
    {
        yield return manager.Initialize();
        
        initializedManagerCount++;
        Debug.Log($"[DependencyManager] {manager.GetType().Name} initialized ({initializedManagerCount}/{managers.Count})");
        
        if (initializedManagerCount >= managers.Count)
        {
            OnAllManagersInitialized();
        }
    }
    
    private void OnAllManagersInitialized()
    {
        Debug.Log("[DependencyManager] All managers initialized! Game is starting...");
    }
}