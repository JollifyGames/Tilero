using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour, IManager
{
    public static SlotManager Instance { get; private set; }
    
    [Header("Slot Configuration")]
    [SerializeField] private MovementSlot[] movementSlots = new MovementSlot[3];
    
    [Header("Test Patterns")]
    [SerializeField] private List<MovementPattern> testPatterns = new List<MovementPattern>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        SetupTestPatterns();
    }
    
    public IEnumerator Initialize()
    {
        InitializeSlots();
        AssignTestPatterns();
        yield return null;
    }
    
    private void InitializeSlots()
    {
        for (int i = 0; i < movementSlots.Length; i++)
        {
            if (movementSlots[i] != null)
            {
                movementSlots[i].Initialize(i);
                movementSlots[i].OnSlotClicked += OnSlotClicked;
            }
        }
        
        Debug.Log($"[SlotManager] Initialized {movementSlots.Length} slots");
    }
    
    private void SetupTestPatterns()
    {
        if (testPatterns.Count == 0)
        {
            MovementPattern pattern1 = new MovementPattern("Forward Rush");
            pattern1.Steps.Add(new Vector2Int(0, 1));
            pattern1.Steps.Add(new Vector2Int(0, 2));
            pattern1.Steps.Add(new Vector2Int(0, 3));
            testPatterns.Add(pattern1);
            
            MovementPattern pattern2 = new MovementPattern("L-Shape");
            pattern2.Steps.Add(new Vector2Int(1, 0));
            pattern2.Steps.Add(new Vector2Int(2, 0));
            pattern2.Steps.Add(new Vector2Int(2, 1));
            testPatterns.Add(pattern2);
            
            MovementPattern pattern3 = new MovementPattern("Diagonal");
            pattern3.Steps.Add(new Vector2Int(1, 0));
            pattern3.Steps.Add(new Vector2Int(1, 1));
            pattern3.Steps.Add(new Vector2Int(2, 1));
            testPatterns.Add(pattern3);
        }
    }
    
    private void AssignTestPatterns()
    {
        Debug.Log($"[SlotManager] Assigning {testPatterns.Count} patterns to {movementSlots.Length} slots");
        
        for (int i = 0; i < movementSlots.Length && i < testPatterns.Count; i++)
        {
            if (movementSlots[i] != null && testPatterns[i] != null)
            {
                movementSlots[i].AssignPattern(testPatterns[i]);
                Debug.Log($"[SlotManager] Assigned pattern '{testPatterns[i].PatternName}' to slot {i}");
            }
            else
            {
                Debug.LogWarning($"[SlotManager] Slot {i} or pattern {i} is null");
            }
        }
    }
    
    private void OnSlotClicked(int slotIndex, MovementPattern pattern)
    {
        if (BoardManager.Instance == null || PlayerController.Instance == null)
        {
            Debug.LogError("[SlotManager] BoardManager or PlayerController not found");
            return;
        }
        
        Debug.Log($"[SlotManager] Executing pattern: {pattern.PatternName}");
        
        Vector2Int currentCell = BoardManager.Instance.GetPlayerCell();
        
        // Direction.Up kullanarak rotation olmadan direkt pattern'i uygula
        List<Vector2Int> absoluteSteps = pattern.GetAbsoluteSteps(currentCell, Direction.Up);
        
        PlayerController.Instance.ExecuteMovementPattern(pattern, absoluteSteps);
    }
    
    public void AssignPatternToSlot(int slotIndex, MovementPattern pattern)
    {
        if (slotIndex >= 0 && slotIndex < movementSlots.Length && movementSlots[slotIndex] != null)
        {
            movementSlots[slotIndex].AssignPattern(pattern);
            Debug.Log($"[SlotManager] Assigned pattern {pattern.PatternName} to slot {slotIndex}");
        }
    }
    
    private void OnDestroy()
    {
        foreach (var slot in movementSlots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnSlotClicked;
            }
        }
    }
}