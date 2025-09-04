using UnityEngine;

[CreateAssetMenu(fileName = "New Character Stats", menuName = "Game/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Character Statistics")]
    [SerializeField] private int hp = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private int defense = 5;
    [SerializeField] private float dodge = 0.1f;
    [SerializeField] private float crit = 0.2f;
    
    public int HP => hp;
    public int Damage => damage;
    public int Defense => defense;
    public float Dodge => dodge;
    public float Crit => crit;
}