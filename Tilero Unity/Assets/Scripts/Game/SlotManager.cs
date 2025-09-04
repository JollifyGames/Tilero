using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour, IManager
{
    public static SlotManager Instance { get; private set; }
    
    [Header("Slot Configuration")]
    [SerializeField] private MovementSlot[] movementSlots = new MovementSlot[3];
    
    [Header("Deck Configuration")]
    [SerializeField] private DeckSO deckSO;
    
    private DeckService deckService;
    private PatternSO[] currentPatterns = new PatternSO[3];
    
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
        InitializeDeckService();
        InitializeSlots();
        DrawInitialCards();
        yield return null;
    }
    
    private void InitializeDeckService()
    {
        if (deckSO != null)
        {
            deckService = new DeckService(deckSO);
            Debug.Log("[SlotManager] Deck service initialized");
        }
        else
        {
            Debug.LogError("[SlotManager] No deck assigned!");
        }
    }
    
    private void InitializeSlots()
    {
        for (int i = 0; i < movementSlots.Length; i++)
        {
            if (movementSlots[i] != null)
            {
                movementSlots[i].Initialize(i);
                movementSlots[i].OnSlotClicked += OnSlotClicked;
                movementSlots[i].OnSlotPressed += OnSlotPressed;
                movementSlots[i].OnSlotReleased += OnSlotReleased;
                movementSlots[i].OnSlotHeld += OnSlotHeld;
            }
        }
        
        Debug.Log($"[SlotManager] Initialized {movementSlots.Length} slots");
    }
    
    private void DrawInitialCards()
    {
        if (deckService == null) return;
        
        for (int i = 0; i < movementSlots.Length; i++)
        {
            DrawCardToSlot(i);
        }
    }
    
    private void DrawCardToSlot(int slotIndex)
    {
        if (deckService == null || slotIndex >= movementSlots.Length) return;
        
        PatternSO drawnCard = deckService.DrawCard();
        if (drawnCard != null && movementSlots[slotIndex] != null)
        {
            currentPatterns[slotIndex] = drawnCard;
            movementSlots[slotIndex].AssignPattern(drawnCard);
            Debug.Log($"[SlotManager] Drew {drawnCard.PatternName} to slot {slotIndex}");
        }
    }
    
    public void RefillSlot(int slotIndex)
    {
        // Used slot'u discard et ve yeni kart çek
        if (currentPatterns[slotIndex] != null && deckService != null)
        {
            deckService.DiscardCard(currentPatterns[slotIndex]);
            currentPatterns[slotIndex] = null;
            
            // Clear the slot visualization
            if (movementSlots[slotIndex] != null)
            {
                movementSlots[slotIndex].AssignPattern(null);
            }
            
            // Draw new card
            DrawCardToSlot(slotIndex);
        }
    }
    
    private void OnSlotClicked(int slotIndex, PatternSO pattern)
    {
        if (BoardManager.Instance == null || PlayerController.Instance == null)
        {
            Debug.LogError("[SlotManager] BoardManager or PlayerController not found");
            return;
        }
        
        Debug.Log($"[SlotManager] Executing pattern: {pattern.PatternName}");
        
        Vector2Int currentCell = BoardManager.Instance.GetPlayerCell();
        
        // Direction.Up kullanarak rotation olmadan direkt pattern'i uygula
        // Convert PatternSO to MovementPattern for compatibility
        MovementPattern movementPattern = pattern.ToMovementPattern();
        List<Vector2Int> absoluteSteps = movementPattern.GetAbsoluteSteps(currentCell, Direction.Up);
        
        PlayerController.Instance.ExecuteMovementPattern(movementPattern, absoluteSteps);
        
        // Refill the used slot
        RefillSlot(slotIndex);
    }
    
    public void AssignPatternToSlot(int slotIndex, PatternSO pattern)
    {
        if (slotIndex >= 0 && slotIndex < movementSlots.Length && movementSlots[slotIndex] != null)
        {
            movementSlots[slotIndex].AssignPattern(pattern);
            Debug.Log($"[SlotManager] Assigned pattern {pattern.PatternName} to slot {slotIndex}");
        }
    }
    
    private void OnSlotPressed(int slotIndex, PatternSO pattern)
    {
        Debug.Log($"[SlotManager] Slot {slotIndex} pressed");
        
        // Press an\u0131nda hemen preview g\u00f6ster
        if (pattern == null || BoardManager.Instance == null || GridManager.Instance == null)
            return;
        
        Vector2Int currentCell = BoardManager.Instance.GetPlayerCell();
        MovementPattern movementPattern = pattern.ToMovementPattern();
        List<Vector2Int> previewPositions = movementPattern.GetAbsoluteSteps(currentCell, Direction.Up);
        
        GridManager.Instance.ShowPatternPreview(previewPositions);
    }
    
    private void OnSlotHeld(int slotIndex, PatternSO pattern)
    {
        // Hold oldu\u011funda bir \u015fey yapmaya gerek yok, preview zaten g\u00f6steriliyor
        Debug.Log($"[SlotManager] Slot {slotIndex} held with pattern: {pattern.PatternName}");
    }
    
    private void OnSlotReleased(int slotIndex, PatternSO pattern)
    {
        Debug.Log($"[SlotManager] Slot {slotIndex} released");
        
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearPatternPreview();
        }
    }
    
    private void OnDestroy()
    {
        foreach (var slot in movementSlots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnSlotClicked;
                slot.OnSlotPressed -= OnSlotPressed;
                slot.OnSlotReleased -= OnSlotReleased;
                slot.OnSlotHeld -= OnSlotHeld;
            }
        }
    }
}