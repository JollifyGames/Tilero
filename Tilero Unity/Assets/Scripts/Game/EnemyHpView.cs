using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyHpView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Canvas hpCanvas;
    
    [Header("Animation Settings")]
    [SerializeField] private float fillAnimationDuration = 0.3f;
    [SerializeField] private Ease fillEase = Ease.OutQuad;
    
    [Header("Display Settings")]
    [SerializeField] private bool hideWhenFull = true;
    
    private EnemyCharacter enemyCharacter;
    private CharacterModel characterModel;
    
    private void Awake()
    {
        enemyCharacter = GetComponentInParent<EnemyCharacter>();
        if (enemyCharacter == null)
        {
            enemyCharacter = GetComponent<EnemyCharacter>();
        }
        
        if (enemyCharacter == null)
        {
            Debug.LogError($"[EnemyHpView] EnemyCharacter not found on {gameObject.name}!");
            enabled = false;
            return;
        }
        
        if (fillImage == null)
        {
            Debug.LogError($"[EnemyHpView] Fill Image not assigned on {gameObject.name}!");
            enabled = false;
            return;
        }
        
        if (hpCanvas == null)
        {
            hpCanvas = GetComponentInChildren<Canvas>();
        }
    }
    
    private void Start()
    {
        if (enemyCharacter != null)
        {
            characterModel = enemyCharacter.Model;
            if (characterModel != null)
            {
                characterModel.OnHpChanged += OnHpChanged;
                
                UpdateHpBar(characterModel.CurrentHp, characterModel.MaxHp, false);
            }
        }
    }
    
    private void OnHpChanged(int currentHp, int maxHp)
    {
        UpdateHpBar(currentHp, maxHp, true);
    }
    
    private void UpdateHpBar(int currentHp, int maxHp, bool animate = true)
    {
        if (fillImage == null) return;
        
        float targetFillAmount = maxHp > 0 ? (float)currentHp / maxHp : 0f;
        
        if (animate)
        {
            fillImage.DOFillAmount(targetFillAmount, fillAnimationDuration)
                .SetEase(fillEase);
        }
        else
        {
            fillImage.fillAmount = targetFillAmount;
        }
        
        if (hideWhenFull && hpCanvas != null)
        {
            bool shouldShow = currentHp < maxHp && currentHp > 0;
            hpCanvas.gameObject.SetActive(shouldShow);
        }
    }
    
    private void OnDestroy()
    {
        if (characterModel != null)
        {
            characterModel.OnHpChanged -= OnHpChanged;
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (fillImage == null)
        {
            fillImage = GetComponentInChildren<Image>();
        }
        
        if (hpCanvas == null)
        {
            hpCanvas = GetComponentInChildren<Canvas>();
        }
    }
}