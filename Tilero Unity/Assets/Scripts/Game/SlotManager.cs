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
        // Used slot'u discard et, AMA YENİ KART ÇEKME
        if (currentPatterns[slotIndex] != null && deckService != null)
        {
            deckService.DiscardCard(currentPatterns[slotIndex]);
            currentPatterns[slotIndex] = null;
            
            // Clear the slot visualization
            if (movementSlots[slotIndex] != null)
            {
                movementSlots[slotIndex].AssignPattern(null);
            }
            
            // Artık burada yeni kart çekmiyor, slot boş kalıyor
        }
    }
    
    public void RefreshAllSlots()
    {
        Debug.Log("[SlotManager] Refreshing all slots for new turn");
        
        // Tüm slotları temizle ve yeniden doldur
        for (int i = 0; i < movementSlots.Length; i++)
        {
            // Eğer slot'ta kart varsa discard et
            if (currentPatterns[i] != null && deckService != null)
            {
                deckService.DiscardCard(currentPatterns[i]);
                currentPatterns[i] = null;
            }
            
            // Yeni kart çek
            DrawCardToSlot(i);
        }
    }
    
    private void OnSlotClicked(int slotIndex, PatternSO pattern)
    {
        if (BoardManager.Instance == null || PlayerController.Instance == null)
        {
            Debug.LogError("[SlotManager] BoardManager or PlayerController not found");
            return;
        }
        
        // WorldManager ile turn kontrolü
        if (WorldManager.Instance != null && !WorldManager.Instance.CanPlayerAct())
        {
            Debug.Log("[SlotManager] Cannot use card - not player turn or player is moving");
            return;
        }
        
        // Energy kontrolü
        if (WorldManager.Instance != null)
        {
            int cost = pattern.Cost;
            if (!WorldManager.Instance.HasEnoughEnergy(cost))
            {
                Debug.Log($"[SlotManager] Not enough energy! Pattern costs {cost}, have {WorldManager.Instance.CurrentPlayerEnergy}");
                return;
            }
            
            // Energy'yi harca
            if (!WorldManager.Instance.SpendEnergy(cost))
            {
                return;
            }
        }
        
        Debug.Log($"[SlotManager] Executing pattern: {pattern.PatternName} (Cost: {pattern.Cost})");
        
        // Get rotation from slot
        int rotation = 0;
        if (movementSlots[slotIndex] != null)
        {
            rotation = movementSlots[slotIndex].GetCurrentRotation();
            Debug.Log($"[SlotManager] Using rotation: {rotation}°");
        }
        
        Vector2Int currentCell = BoardManager.Instance.GetPlayerCell();
        
        // Apply rotation to pattern for execution
        List<Vector2Int> absoluteSteps = new List<Vector2Int>();
        foreach (var step in pattern.Steps)
        {
            Vector2Int rotatedPos = RotateVector(step.position, rotation);
            absoluteSteps.Add(currentCell + rotatedPos);
        }
        
        // PatternSO'yu direkt gönder, PieceType bilgisi için
        PlayerController.Instance.ExecuteMovementPattern(pattern, absoluteSteps);
        
        // Sadece slot'u temizle, yeni kart çekme
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
        
        // Press anında hemen preview göster
        if (pattern == null || BoardManager.Instance == null || GridManager.Instance == null)
            return;
        
        // Get rotation from slot
        int rotation = 0;
        if (movementSlots[slotIndex] != null)
        {
            rotation = movementSlots[slotIndex].GetCurrentRotation();
        }
        
        Vector2Int currentCell = BoardManager.Instance.GetPlayerCell();
        
        // Apply rotation to pattern for preview
        List<Vector2Int> previewPositions = new List<Vector2Int>();
        foreach (var step in pattern.Steps)
        {
            Vector2Int rotatedPos = RotateVector(step.position, rotation);
            previewPositions.Add(currentCell + rotatedPos);
        }
        
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
    
    public bool CanPlayAnyCard()
    {
        if (WorldManager.Instance == null) return false;
        
        int currentEnergy = WorldManager.Instance.CurrentPlayerEnergy;
        
        // Tüm slotları kontrol et
        for (int i = 0; i < currentPatterns.Length; i++)
        {
            if (currentPatterns[i] != null && currentPatterns[i].Cost <= currentEnergy)
            {
                return true; // En az bir kart oynanabilir
            }
        }
        
        return false; // Hiçbir kart oynanamaz
    }
    
    private Vector2Int RotateVector(Vector2Int pos, int angle)
    {
        switch (angle)
        {
            case 0:
                return pos;
            case 90:
                return new Vector2Int(-pos.y, pos.x);
            case 180:
                return new Vector2Int(-pos.x, -pos.y);
            case 270:
                return new Vector2Int(pos.y, -pos.x);
            default:
                return pos;
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