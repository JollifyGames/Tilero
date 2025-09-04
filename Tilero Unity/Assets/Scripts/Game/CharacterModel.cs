using UnityEngine;

[System.Serializable]
public class CharacterModel
{
    private int currentHp;
    private int maxHp;
    private int damage;
    private int defense;
    private int baseDefense;
    private float dodge;
    private float crit;
    
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public int Damage => damage;
    public int Defense => defense;
    public float Dodge => dodge;
    public float Crit => crit;
    
    public CharacterModel(CharacterStats stats)
    {
        if (stats != null)
        {
            maxHp = stats.HP;
            currentHp = maxHp;
            damage = stats.Damage;
            defense = stats.Defense;
            baseDefense = stats.Defense;
            dodge = stats.Dodge;
            crit = stats.Crit;
        }
        else
        {
            Debug.LogError("[CharacterModel] CharacterStats is null!");
            maxHp = 100;
            currentHp = maxHp;
            damage = 10;
            defense = 0;
            baseDefense = 0;
            dodge = 0f;
            crit = 0f;
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        currentHp -= damageAmount;
        currentHp = Mathf.Max(0, currentHp);
        Debug.Log($"[CharacterModel] Took {damageAmount} damage. HP: {currentHp}/{maxHp}");
    }
    
    public void Heal(int healAmount)
    {
        currentHp += healAmount;
        currentHp = Mathf.Min(maxHp, currentHp);
        Debug.Log($"[CharacterModel] Healed {healAmount}. HP: {currentHp}/{maxHp}");
    }
    
    public bool IsDead()
    {
        return currentHp <= 0;
    }
    
    public float GetHealthPercentage()
    {
        return maxHp > 0 ? (float)currentHp / maxHp : 0f;
    }
    
    public void AddTemporaryDefense(int amount)
    {
        defense += amount;
        Debug.Log($"[CharacterModel] Added {amount} temporary defense. Defense: {defense}");
    }
    
    public void ResetDefense()
    {
        defense = baseDefense;
        Debug.Log($"[CharacterModel] Defense reset to base value: {defense}");
    }
}