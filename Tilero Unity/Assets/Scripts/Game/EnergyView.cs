using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyView : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Button endTurnButton;
    
    [Header("Display Format")]
    [SerializeField] private string displayFormat = "{0}/{1}"; // Current/Max format
    [SerializeField] private bool showOnlyCurrentEnergy = false;
    
    private int lastKnownEnergy = -1;
    private int lastKnownMaxEnergy = -1;
    
    private void Start()
    {
        if (energyText == null)
        {
            energyText = GetComponent<TextMeshProUGUI>();
        }
        
        if (energyText == null)
        {
            Debug.LogError("[EnergyView] TextMeshProUGUI component not found!");
            enabled = false;
            return;
        }
        
        // Setup end turn button
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }
        
        UpdateEnergyDisplay();
    }
    
    private void OnDestroy()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(OnEndTurnButtonClicked);
        }
    }
    
    private void OnEndTurnButtonClicked()
    {
        if (WorldManager.Instance != null)
        {
            Debug.Log("[EnergyView] End Turn button clicked");
            WorldManager.Instance.EndPlayerTurn();
        }
    }
    
    private void Update()
    {
        if (WorldManager.Instance == null) return;
        
        int currentEnergy = WorldManager.Instance.CurrentPlayerEnergy;
        int maxEnergy = WorldManager.Instance.PlayerEnergyBase;
        
        // Check if energy changed
        if (currentEnergy != lastKnownEnergy || maxEnergy != lastKnownMaxEnergy)
        {
            lastKnownEnergy = currentEnergy;
            lastKnownMaxEnergy = maxEnergy;
            UpdateEnergyDisplay();
        }
        
        // Update button interactability
        if (endTurnButton != null)
        {
            // Button sadece player turn'ünde aktif olsun
            endTurnButton.interactable = WorldManager.Instance.IsPlayerTurn && 
                                         WorldManager.Instance.CurrentTurn != TurnState.Processing;
        }
    }
    
    private void UpdateEnergyDisplay()
    {
        if (energyText == null || WorldManager.Instance == null) return;
        
        int currentEnergy = WorldManager.Instance.CurrentPlayerEnergy;
        int maxEnergy = WorldManager.Instance.PlayerEnergyBase;
        
        if (showOnlyCurrentEnergy)
        {
            energyText.text = currentEnergy.ToString();
        }
        else
        {
            energyText.text = string.Format(displayFormat, currentEnergy, maxEnergy);
        }
        
        // Optional: Change color based on energy level
        UpdateEnergyColor(currentEnergy, maxEnergy);
    }
    
    private void UpdateEnergyColor(int current, int max)
    {
        if (energyText == null) return;
        
        float energyPercentage = max > 0 ? (float)current / max : 0f;
        
        if (energyPercentage >= 0.75f)
        {
            energyText.color = Color.white; // Full energy
        }
        else if (energyPercentage >= 0.5f)
        {
            energyText.color = new Color(1f, 1f, 0.5f); // Yellow-ish
        }
        else if (energyPercentage >= 0.25f)
        {
            energyText.color = new Color(1f, 0.7f, 0.3f); // Orange
        }
        else
        {
            energyText.color = new Color(1f, 0.4f, 0.4f); // Red-ish
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (energyText == null)
        {
            energyText = GetComponent<TextMeshProUGUI>();
        }
    }
}