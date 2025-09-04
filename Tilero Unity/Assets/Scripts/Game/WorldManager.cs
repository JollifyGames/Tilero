using System.Collections;
using UnityEngine;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
    Processing
}

public class WorldManager : MonoBehaviour, IManager
{
    public static WorldManager Instance { get; private set; }
    
    [Header("Turn Management")]
    [SerializeField] private TurnState currentTurn = TurnState.PlayerTurn;
    
    public TurnState CurrentTurn => currentTurn;
    public bool IsPlayerTurn => currentTurn == TurnState.PlayerTurn;
    
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
        Debug.Log("[WorldManager] Initialized - Starting with Player Turn");
        StartPlayerTurn();
        yield return null;
    }
    
    private void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        Debug.Log("[WorldManager] === PLAYER TURN START ===");
        
        // Reset player defense buff
        if (PlayerController.Instance != null)
        {
            PlayerCharacter playerChar = PlayerController.Instance.GetComponent<PlayerCharacter>();
            if (playerChar != null)
            {
                playerChar.ResetDefense();
            }
        }
    }
    
    public void OnPlayerActionComplete()
    {
        if (currentTurn != TurnState.PlayerTurn)
        {
            Debug.LogWarning("[WorldManager] Player action complete called but it's not player turn!");
            return;
        }
        
        Debug.Log("[WorldManager] Player action complete, switching to Enemy turn");
        StartEnemyTurn();
    }
    
    private void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("[WorldManager] === ENEMY TURN START ===");
        
        if (EnemyManager.Instance != null)
        {
            StartCoroutine(ProcessEnemyTurn());
        }
        else
        {
            Debug.LogWarning("[WorldManager] EnemyManager not found, skipping enemy turn");
            StartPlayerTurn();
        }
    }
    
    private IEnumerator ProcessEnemyTurn()
    {
        currentTurn = TurnState.Processing;
        
        yield return EnemyManager.Instance.ProcessAllEnemyTurns();
        
        Debug.Log("[WorldManager] All enemies processed their turns");
        StartPlayerTurn();
    }
    
    public bool CanPlayerAct()
    {
        return currentTurn == TurnState.PlayerTurn && 
               PlayerController.Instance != null && 
               !PlayerController.Instance.IsMoving;
    }
}