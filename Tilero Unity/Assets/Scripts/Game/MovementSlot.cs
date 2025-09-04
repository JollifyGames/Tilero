using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class MovementSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    [SerializeField] private int slotIndex;
    [SerializeField] private MovementPattern assignedPattern;
    
    [Header("Visualization Settings")]
    [SerializeField] private GameObject cellVisualizationPrefab;
    [SerializeField] private Transform visualizationContainer;
    [SerializeField] private float cellDisplaySize = 30f; // Cell'in görsel boyutu (100'e bölünecek)
    [SerializeField] private float cellSpacing = 40f; // Cell'ler arası mesafe (100'e bölünecek)
    [SerializeField] private bool autoResize = true; // Canvas'a sığmazsa otomatik küçült
    [SerializeField] private float maxCanvasSize = 200f; // Max canvas boyutu (100'e bölünecek = 2.0f)
    
    [Header("Hold Settings")]
    [SerializeField] private float holdThreshold = 0.5f;
    
    public event Action<int, MovementPattern> OnSlotClicked;
    public event Action<int, MovementPattern> OnSlotPressed;
    public event Action<int, MovementPattern> OnSlotReleased;
    public event Action<int, MovementPattern> OnSlotHeld;
    
    public int SlotIndex => slotIndex;
    public MovementPattern AssignedPattern => assignedPattern;
    
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
    
    public void AssignPattern(MovementPattern pattern)
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
        
        if (cellVisualizationPrefab == null)
        {
            Debug.LogError($"[MovementSlot {slotIndex}] Cell Visualization Prefab is not assigned!");
            return;
        }
        
        Debug.Log($"[MovementSlot {slotIndex}] Creating visualization for pattern: {assignedPattern.PatternName} with {assignedPattern.Steps.Count} steps");
        
        // Resize faktörünü hesapla (gerekirse)
        float scaleFactor = CalculateScaleFactor();
        
        // Pattern'in merkez offset'ini hesapla
        Vector2 centerOffset = CalculatePatternCenterOffset(scaleFactor);
        
        // Player pozisyonunu (0,0) olarak göster
        CreateVisualizationCell(Vector2Int.zero, true, centerOffset, scaleFactor);
        
        // Pattern step'lerini göster
        foreach (var step in assignedPattern.Steps)
        {
            CreateVisualizationCell(step, false, centerOffset, scaleFactor);
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
            minX = Mathf.Min(minX, step.x);
            maxX = Mathf.Max(maxX, step.x);
            minY = Mathf.Min(minY, step.y);
            maxY = Mathf.Max(maxY, step.y);
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
            minX = Mathf.Min(minX, step.x);
            maxX = Mathf.Max(maxX, step.x);
            minY = Mathf.Min(minY, step.y);
            maxY = Mathf.Max(maxY, step.y);
        }
        
        // Pattern'in merkezini hesapla
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;
        
        // Merkeze offset olarak dön (scale faktörü ile)
        return new Vector2(-centerX * cellSpacing * scaleFactor, -centerY * cellSpacing * scaleFactor);
    }
    
    private void CreateVisualizationCell(Vector2Int gridPos, bool isPlayerStart, Vector2 centerOffset, float scaleFactor)
    {
        GameObject cell = Instantiate(cellVisualizationPrefab, visualizationContainer);
        
        // World Space Canvas için direkt transform kullan
        if (cell != null)
        {
            // 0,0 merkez olacak şekilde hesapla ve 100'e böl
            float xPos = ((gridPos.x * cellSpacing * scaleFactor) + centerOffset.x) / 100f;
            float yPos = ((gridPos.y * cellSpacing * scaleFactor) + centerOffset.y) / 100f;
            
            cell.transform.localPosition = new Vector3(xPos, yPos, 0);
            
            // RectTransform varsa width/height'ı ayarla
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float finalCellSize = cellDisplaySize * scaleFactor;
                rectTransform.sizeDelta = Vector2.one * (finalCellSize / 100f);
            }
            
            // Player başlangıç hücresini farklı renklendir
            if (isPlayerStart)
            {
                var image = cell.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Yeşilimsi
                }
            }
        }
        
        visualizationCells.Add(cell);
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