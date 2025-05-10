using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private List<CardData> startingDeckData;

    private Stack<Card> drawPile;
    private List<Card> discardPile = new List<Card>();

    public void InitializeDeck()
    {
        CardFactory.ResetCardIDCounter();

        // Create instances of each CardData
        var cards = CardFactory.CreateCards(startingDeckData);
        ShuffleIntoDrawPile(cards);
    }

    private void ShuffleIntoDrawPile(IEnumerable<Card> cards)
    {
        var list = new List<Card>(cards);
        // simple Fisher–Yates
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
        drawPile = new Stack<Card>(list);
    }

    /// <summary> Draws one card. If draw pile is empty, reshuffle discard into draw. </summary>
    public Card DrawCard()
    {
        if (drawPile.Count == 0 && discardPile.Count > 0)
        {
            ShuffleIntoDrawPile(discardPile);
            discardPile.Clear();
        }

        return drawPile.Count > 0 ? drawPile.Pop() : null;
    }

    public void Discard(Card card)
    {
        discardPile.Add(card);
    }

    public int DrawCount => drawPile.Count;
    public int DiscardCount => discardPile.Count;
}
