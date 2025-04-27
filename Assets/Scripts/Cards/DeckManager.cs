using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }
    public List<Card> startingDeck;
    public int handSize = 5;
    private List<Card> deck;
    public List<Card> Hand { get; private set; }
    private List<Card> discardPile;
    public HandUIManager handUIManager;
    public TMPro.TMP_Text deckCountText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        deck = new List<Card>(startingDeck);
        Hand = new List<Card>();
        discardPile = new List<Card>();
        ShuffleDeck();
        DrawInitialHand();
        UpdateDeckCountUI();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            Card temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void DrawInitialHand()
    {
        for (int i = 0; i < handSize; i++)
            DrawCard();
    }

    public void DrawCard()
    {
        if (deck.Count == 0)
        {
            deck = new List<Card>(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
        if (deck.Count > 0)
        {
            Card drawnCard = deck[0];
            deck.RemoveAt(0);
            Hand.Add(drawnCard);
            if (handUIManager != null)
                handUIManager.UpdateHand(Hand);
            UpdateDeckCountUI();
        }
    }

    public void DiscardCard(Card card)
    {
        if (Hand.Contains(card))
        {
            Hand.Remove(card);
            discardPile.Add(card);
            if (handUIManager != null)
                handUIManager.UpdateHand(Hand);
            UpdateDeckCountUI();
        }
    }

    void UpdateDeckCountUI()
    {
        if (deckCountText != null) // Null check
            deckCountText.text = $"Deck: {deck.Count}";
        else
            Debug.LogWarning("DeckCountText is not assigned in DeckManager.");
    }
}