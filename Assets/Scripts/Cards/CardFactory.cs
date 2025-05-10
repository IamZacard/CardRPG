using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Factory class responsible for creating card instances from CardData
/// </summary>
public static class CardFactory
{
    // Counter for generating unique card instance IDs
    private static int nextCardID = 1;

    /// <summary>
    /// Creates a new card instance based on CardData
    /// </summary>
    /// <param name="cardData">The card data to instantiate from</param>
    /// <returns>A new Card instance with unique ID</returns>
    public static Card CreateCard(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("CardFactory: Cannot create card from null CardData!");
            return null;
        }

        int id = GetNextCardID();
        Card newCard = new Card(cardData, id);

        Debug.Log($"Created card: {cardData.cardName} with ID: {id}");
        return newCard;
    }

    /// <summary>
    /// Creates multiple cards from a list of CardData
    /// </summary>
    /// <param name="cardDataList">List of card data to instantiate</param>
    /// <returns>List of new Card instances</returns>
    public static List<Card> CreateCards(List<CardData> cardDataList)
    {
        List<Card> result = new List<Card>();

        foreach (CardData data in cardDataList)
        {
            Card card = CreateCard(data);
            if (card != null)
            {
                result.Add(card);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates multiple copies of the same card
    /// </summary>
    /// <param name="cardData">Card data to instantiate</param>
    /// <param name="count">Number of copies to create</param>
    /// <returns>List of new Card instances</returns>
    public static List<Card> CreateCardCopies(CardData cardData, int count)
    {
        List<Card> result = new List<Card>();

        for (int i = 0; i < count; i++)
        {
            Card card = CreateCard(cardData);
            if (card != null)
            {
                result.Add(card);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a UI representation for a card instance
    /// </summary>
    /// <param name="card">The card instance to create UI for</param>
    /// <param name="prefab">The card UI prefab to instantiate</param>
    /// <param name="parent">The parent transform for the UI</param>
    /// <returns>The instantiated CardUI component</returns>
    public static CardUI CreateCardUI(Card card, GameObject prefab, Transform parent)
    {
        if (card == null || prefab == null)
        {
            Debug.LogError("CardFactory: Cannot create UI with null card or prefab!");
            return null;
        }

        GameObject uiObject = Object.Instantiate(prefab, parent);
        CardUI cardUI = uiObject.GetComponent<CardUI>();

        if (cardUI == null)
        {
            Debug.LogError("CardFactory: Prefab does not have a CardUI component!");
            Object.Destroy(uiObject);
            return null;
        }

        cardUI.Initialize(card);
        return cardUI;
    }

    /// <summary>
    /// Gets the next unique card ID and increments the counter
    /// </summary>
    /// <returns>Unique card ID</returns>
    public static int GetNextCardID()
    {
        return nextCardID++;
    }

    /// <summary>
    /// Resets the card ID counter (useful for starting new games)
    /// </summary>
    public static void ResetCardIDCounter()
    {
        nextCardID = 1;
    }
}