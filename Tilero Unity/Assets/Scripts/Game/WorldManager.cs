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
    
    [Header("Energy System")]
    [SerializeField] private int playerEnergyBase = 4;
    private int currentPlayerEnergy;
    
    public TurnState CurrentTurn => currentTurn;
    public bool IsPlayerTurn => currentTurn == TurnState.PlayerTurn;
    public int CurrentPlayerEnergy => currentPlayerEnergy;
    public int PlayerEnergyBase => playerEnergyBase;
    
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
        currentPlayerEnergy = playerEnergyBase;
        StartPlayerTurn();
        yield return null;
    }
    
    private void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        
        // Reset player energy
        currentPlayerEnergy = playerEnergyBase;
        Debug.Log($"[WorldManager] === PLAYER TURN START === Energy: {currentPlayerEnergy}/{playerEnergyBase}");
        
        // Reset player defense buff
        if (PlayerController.Instance != null)
        {
            PlayerCharacter playerChar = PlayerController.Instance.GetComponent<PlayerCharacter>();
            if (playerChar != null)
            {
                playerChar.ResetDefense();
            }
        }
        
        // Refresh all slots
        if (SlotManager.Instance != null)
        {
            SlotManager.Instance.RefreshAllSlots();
        }
    }
    
    public bool HasEnoughEnergy(int cost)
    {
        return currentPlayerEnergy >= cost;
    }
    
    public bool SpendEnergy(int cost)
    {
        if (!HasEnoughEnergy(cost))
        {
            Debug.LogWarning($"[WorldManager] Not enough energy! Need {cost}, have {currentPlayerEnergy}");
            return false;
        }
        
        currentPlayerEnergy -= cost;
        Debug.Log($"[WorldManager] Spent {cost} energy. Remaining: {currentPlayerEnergy}/{playerEnergyBase}");
        
        // Check if player can still play any cards
        CheckForAutoEndTurn();
        
        return true;
    }
    
    private void CheckForAutoEndTurn()
    {
        // Eğer energy 0 veya tüm kartlar çok pahalıysa turn'ü otomatik bitir
        if (currentPlayerEnergy <= 0)
        {
            Debug.Log("[WorldManager] Energy depleted, auto-ending turn");
            StartCoroutine(DelayedEndTurn());
        }
        else if (SlotManager.Instance != null && !SlotManager.Instance.CanPlayAnyCard())
        {
            Debug.Log("[WorldManager] No playable cards with remaining energy, auto-ending turn");
            StartCoroutine(DelayedEndTurn());
        }
    }
    
    private IEnumerator DelayedEndTurn()
    {
        // Kısa bir delay, animasyonların bitmesini bekle
        yield return new WaitForSeconds(0.5f);
        
        if (currentTurn == TurnState.PlayerTurn)
        {
            OnPlayerActionComplete();
        }
    }
    
    public void EndPlayerTurn()
    {
        // Manual turn end
        if (currentTurn == TurnState.PlayerTurn)
        {
            Debug.Log("[WorldManager] Player manually ending turn");
            OnPlayerActionComplete();
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