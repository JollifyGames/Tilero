using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Deck", menuName = "Game/Deck")]
public class DeckSO : ScriptableObject
{
    [Header("Deck Info")]
    [SerializeField] private string deckName;
    [TextArea(2, 4)]
    [SerializeField] private string description;
    
    [Header("Cards")]
    [SerializeField] private List<DeckCard> cards = new List<DeckCard>();
    
    public string DeckName => deckName;
    public string Description => description;
    public List<DeckCard> Cards => cards;
    
    [System.Serializable]
    public class DeckCard
    {
        [SerializeField] private PatternSO pattern;
        [SerializeField] private int count = 1;
        
        public PatternSO Pattern => pattern;
        public int Count => count;
    }
    
    // Tüm kartları tek liste halinde döndürür (count kadar çoğaltılmış)
    public List<PatternSO> GetAllCards()
    {
        List<PatternSO> allCards = new List<PatternSO>();
        
        foreach (var card in cards)
        {
            if (card.Pattern != null)
            {
                for (int i = 0; i < card.Count; i++)
                {
                    allCards.Add(card.Pattern);
                }
            }
        }
        
        return allCards;
    }
}