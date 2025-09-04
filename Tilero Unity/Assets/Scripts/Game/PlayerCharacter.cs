using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [Header("Character Configuration")]
    [SerializeField] private CharacterStats characterStats;
    
    [Header("PieceType Damage Multipliers")]
    [Tooltip("Basic piece type damage multiplier")]
    [SerializeField] private float basicMultiplier = 1f;
    [Tooltip("Attack piece type damage multiplier")] 
    [SerializeField] private float attackMultiplier = 2f;
    [Tooltip("Special piece type damage multiplier")]
    [SerializeField] private float specialMultiplier = 3f;
    
    [Header("Defense Buff")]
    [Tooltip("Defense bonus when standing on Defense piece type")]
    [SerializeField] private int defenseBonus = 5;
    
    private CharacterModel model;
    
    public CharacterStats Stats => characterStats;
    public CharacterModel Model => model;
    
    public float GetDamageMultiplier(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Basic: return basicMultiplier;
            case PieceType.Attack: return attackMultiplier;
            case PieceType.Special: return specialMultiplier;
            case PieceType.Defense:
            case PieceType.Player:
                // Defense ve Player attack yapmaz, ama method çağrılırsa 1x döner
                return 1f;
            default: return 1f;
        }
    }
    
    private void Awake()
    {
        InitializeModel();
    }
    
    private void InitializeModel()
    {
        if (characterStats != null)
        {
            model = new CharacterModel(characterStats);
            Debug.Log($"[PlayerCharacter] Model initialized - HP: {model.CurrentHp}/{model.MaxHp}, Damage: {model.Damage}");
        }
        else
        {
            Debug.LogWarning("[PlayerCharacter] No CharacterStats assigned!");
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (model != null)
        {
            model.TakeDamage(damage);
            
            if (model.IsDead())
            {
                Debug.Log("[PlayerCharacter] Player died!");
                // Player death handling can be added later
            }
        }
    }
    
    public void ApplyDefenseBuff()
    {
        if (model != null)
        {
            model.AddTemporaryDefense(defenseBonus);
        }
    }
    
    public void ResetDefense()
    {
        if (model != null)
        {
            model.ResetDefense();
        }
    }
}