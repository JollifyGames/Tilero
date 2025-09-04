using System.Collections.Generic;
using UnityEngine;

public class DeckService
{
    private DeckSO deckSO;
    private Queue<PatternSO> drawPile;
    private List<PatternSO> discardPile;
    
    public DeckService(DeckSO deck)
    {
        deckSO = deck;
        drawPile = new Queue<PatternSO>();
        discardPile = new List<PatternSO>();
        
        InitializeDeck();
    }
    
    private void InitializeDeck()
    {
        if (deckSO == null)
        {
            Debug.LogError("[DeckService] No deck assigned!");
            return;
        }
        
        List<PatternSO> allCards = new List<PatternSO>();
        List<PatternSO> originalCards = deckSO.GetAllCards();
        
        // Her kart için 4 rotasyon ekle (0°, 90°, 180°, 270°)
        foreach (var card in originalCards)
        {
            // 0° - Original
            allCards.Add(card);
            
            // 90°
            PatternSO rotated90 = RotatedPatternSO.CreateRotatedPattern(card, 90);
            allCards.Add(rotated90);
            
            // 180°
            PatternSO rotated180 = RotatedPatternSO.CreateRotatedPattern(card, 180);
            allCards.Add(rotated180);
            
            // 270°
            PatternSO rotated270 = RotatedPatternSO.CreateRotatedPattern(card, 270);
            allCards.Add(rotated270);
            
            Debug.Log($"[DeckService] Added 4 rotations of {card.PatternName}");
        }
        
        ShuffleAndRefillDeck(allCards);
        
        Debug.Log($"[DeckService] Deck initialized with {drawPile.Count} cards ({originalCards.Count} unique patterns x 4 rotations)");
    }
    
    private void ShuffleAndRefillDeck(List<PatternSO> cards)
    {
        drawPile.Clear();
        
        // Fisher-Yates shuffle
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            PatternSO temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
        
        // Add shuffled cards to draw pile
        foreach (var card in cards)
        {
            drawPile.Enqueue(card);
        }
    }
    
    public PatternSO DrawCard()
    {
        // If draw pile is empty, reshuffle discard pile
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.LogWarning("[DeckService] Both draw and discard piles are empty!");
                return null;
            }
            
            Debug.Log("[DeckService] Draw pile empty, reshuffling discard pile");
            ShuffleAndRefillDeck(discardPile);
            discardPile.Clear();
        }
        
        PatternSO drawnCard = drawPile.Dequeue();
        Debug.Log($"[DeckService] Drew card: {drawnCard.PatternName} (Cards left: {drawPile.Count})");
        return drawnCard;
    }
    
    public void DiscardCard(PatternSO card)
    {
        if (card != null)
        {
            discardPile.Add(card);
            Debug.Log($"[DeckService] Discarded card: {card.PatternName}");
        }
    }
    
    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }
    
    public int GetDiscardPileCount()
    {
        return discardPile.Count;
    }
}