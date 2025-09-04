using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class MovementSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    [SerializeField] private int slotIndex;
    private PatternSO assignedPattern;
    
    [Header("Visualization Settings")]
    [SerializeField] private Transform visualizationContainer;
    [SerializeField] private float cellDisplaySize = 30f; // Cell'in görsel boyutu (100'e bölünecek)
    [SerializeField] private float cellSpacing = 40f; // Cell'ler arası mesafe (100'e bölünecek)
    [SerializeField] private bool autoResize = true; // Canvas'a sığmazsa otomatik küçült
    [SerializeField] private float maxCanvasSize = 200f; // Max canvas boyutu (100'e bölünecek = 2.0f)
    
    [Header("Piece Type Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject basicPrefab;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private GameObject defensePrefab;
    [SerializeField] private GameObject specialPrefab;
    
    [Header("Hold Settings")]
    [SerializeField] private float holdThreshold = 0.5f;
    
    public event Action<int, PatternSO> OnSlotClicked;
    public event Action<int, PatternSO> OnSlotPressed;
    public event Action<int, PatternSO> OnSlotReleased;
    public event Action<int, PatternSO> OnSlotHeld;
    
    public int SlotIndex => slotIndex;
    public PatternSO AssignedPattern => assignedPattern;
    
    private bool isPressed = false;
    private float pressTime = 0f;
    private bool holdEventTriggered = false;
    private Coroutine holdCheckCoroutine;
    private List<GameObject> visualizationCells = new List<GameObject>();
    
    public void Initialize(int index)
    {
        slotIndex = index;
        
        if (visualizationContainer == null)
        {
            visualizationContainer = transform;
        }
    }
    
    public void AssignPattern(PatternSO pattern)
    {
        assignedPattern = pattern;
        Debug.Log($"[MovementSlot {slotIndex}] Pattern assigned: {pattern?.PatternName}");
        UpdatePatternVisualization();
    }
    
    private void UpdatePatternVisualization()
    {
        ClearVisualization();
        
        if (assignedPattern == null)
        {
            Debug.LogWarning($"[MovementSlot {slotIndex}] No pattern assigned");
            return;
        }
        
        Debug.Log($"[MovementSlot {slotIndex}] Creating visualization for pattern: {assignedPattern.PatternName} with {assignedPattern.Steps.Count} steps");
        
        // Resize faktörünü hesapla (gerekirse)
        float scaleFactor = CalculateScaleFactor();
        
        // Pattern'in merkez offset'ini hesapla
        Vector2 centerOffset = CalculatePatternCenterOffset(scaleFactor);
        
        // Pattern step'lerini göster (pieceType ile birlikte)
        foreach (var step in assignedPattern.Steps)
        {
            CreateVisualizationCell(step, centerOffset, scaleFactor);
        }
    }
    
    private float CalculateScaleFactor()
    {
        if (!autoResize) return 1f;
        
        if (assignedPattern == null || assignedPattern.Steps.Count == 0)
            return 1f;
        
        // Pattern'in boyutlarını bul
        int minX = 0, maxX = 0;
        int minY = 0, maxY = 0;
        
        foreach (var step in assignedPattern.Steps)
        {
            minX = Mathf.Min(minX, step.position.x);
            maxX = Mathf.Max(maxX, step.position.x);
            minY = Mathf.Min(minY, step.position.y);
            maxY = Mathf.Max(maxY, step.position.y);
        }
        
        float patternWidth = (maxX - minX + 1) * cellSpacing;
        float patternHeight = (maxY - minY + 1) * cellSpacing;
        
        // Canvas'a sığıyor mu kontrol et
        if (patternWidth <= maxCanvasSize && patternHeight <= maxCanvasSize)
            return 1f;
        
        // Sığmıyorsa scale factor hesapla
        float scaleX = maxCanvasSize / patternWidth;
        float scaleY = maxCanvasSize / patternHeight;
        return Mathf.Min(scaleX, scaleY);
    }
    
    private Vector2 CalculatePatternCenterOffset(float scaleFactor)
    {
        if (assignedPattern == null || assignedPattern.Steps.Count == 0)
            return Vector2.zero;
        
        // Pattern'in min ve max değerlerini bul
        int minX = 0, maxX = 0;
        int minY = 0, maxY = 0;
        
        foreach (var step in assignedPattern.Steps)
        {
            minX = Mathf.Min(minX, step.position.x);
            maxX = Mathf.Max(maxX, step.position.x);
            minY = Mathf.Min(minY, step.position.y);
            maxY = Mathf.Max(maxY, step.position.y);
        }
        
        // Pattern'in merkezini hesapla
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;
        
        // Merkeze offset olarak dön (scale faktörü ile)
        return new Vector2(-centerX * cellSpacing * scaleFactor, -centerY * cellSpacing * scaleFactor);
    }
    
    private void CreateVisualizationCell(PatternSO.PatternStep step, Vector2 centerOffset, float scaleFactor)
    {
        GameObject prefab = GetPrefabForPieceType(step.pieceType);
        if (prefab == null)
        {
            Debug.LogWarning($"[MovementSlot {slotIndex}] No prefab assigned for PieceType: {step.pieceType}");
            return;
        }
        
        GameObject cell = Instantiate(prefab, visualizationContainer);
        
        // World Space Canvas için direkt transform kullan
        if (cell != null)
        {
            // 0,0 merkez olacak şekilde hesapla ve 100'e böl
            float xPos = ((step.position.x * cellSpacing * scaleFactor) + centerOffset.x) / 100f;
            float yPos = ((step.position.y * cellSpacing * scaleFactor) + centerOffset.y) / 100f;
            
            cell.transform.localPosition = new Vector3(xPos, yPos, 0);
            
            // RectTransform varsa width/height'ı ayarla
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float finalCellSize = cellDisplaySize * scaleFactor;
                rectTransform.sizeDelta = Vector2.one * (finalCellSize / 100f);
            }
        }
        
        visualizationCells.Add(cell);
    }
    
    private GameObject GetPrefabForPieceType(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Player:
                return playerPrefab;
            case PieceType.Basic:
                return basicPrefab;
            case PieceType.Attack:
                return attackPrefab;
            case PieceType.Defense:
                return defensePrefab;
            case PieceType.Special:
                return specialPrefab;
            default:
                return basicPrefab;
        }
    }
    
    private void ClearVisualization()
    {
        foreach (var cell in visualizationCells)
        {
            if (cell != null)
                Destroy(cell);
        }
        visualizationCells.Clear();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (assignedPattern == null || assignedPattern.Steps.Count == 0) return;
        
        isPressed = true;
        pressTime = Time.time;
        holdEventTriggered = false;
        
        OnSlotPressed?.Invoke(slotIndex, assignedPattern);
        
        if (holdCheckCoroutine != null)
            StopCoroutine(holdCheckCoroutine);
        holdCheckCoroutine = StartCoroutine(CheckForHold());
        
        Debug.Log($"[MovementSlot] Pressed slot {slotIndex}");
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        
        isPressed = false;
        float pressDuration = Time.time - pressTime;
        
        if (holdCheckCoroutine != null)
        {
            StopCoroutine(holdCheckCoroutine);
            holdCheckCoroutine = null;
        }
        
        OnSlotReleased?.Invoke(slotIndex, assignedPattern);
        
        if (!holdEventTriggered && pressDuration < holdThreshold)
        {
            OnSlotClicked?.Invoke(slotIndex, assignedPattern);
            Debug.Log($"[MovementSlot] Clicked slot {slotIndex} (quick tap)");
        }
        else
        {
            Debug.Log($"[MovementSlot] Released slot {slotIndex} after hold");
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // OnPointerUp handles the click logic now
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Visual feedback can be added here
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPressed)
        {
            isPressed = false;
            if (holdCheckCoroutine != null)
            {
                StopCoroutine(holdCheckCoroutine);
                holdCheckCoroutine = null;
            }
            OnSlotReleased?.Invoke(slotIndex, assignedPattern);
        }
    }
    
    private IEnumerator CheckForHold()
    {
        yield return new WaitForSeconds(holdThreshold);
        
        if (isPressed && !holdEventTriggered)
        {
            holdEventTriggered = true;
            OnSlotHeld?.Invoke(slotIndex, assignedPattern);
            Debug.Log($"[MovementSlot] Held slot {slotIndex} with pattern: {assignedPattern.PatternName}");
        }
    }
    
    private void OnDestroy()
    {
        if (holdCheckCoroutine != null)
        {
            StopCoroutine(holdCheckCoroutine);
        }
        
        ClearVisualization();
    }
}