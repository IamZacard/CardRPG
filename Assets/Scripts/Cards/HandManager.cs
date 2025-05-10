using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Transform handZone; // a RectTransform under your canvas

    private List<Card> hand = new List<Card>();
    private List<CardUI> handUI = new List<CardUI>();
    private int maxHandSize = 10;

    public void DrawToHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (hand.Count >= maxHandSize) break;

            var card = deckManager.DrawCard();
            if (card == null) break;

            hand.Add(card);

            var ui = CardFactory.CreateCardUI(card, cardUIPrefab, handZone);
            if (ui != null)
            {
                handUI.Add(ui);
            }
            else
            {
                Debug.LogWarning($"HandManager: Failed to create CardUI for card '{card.cardData.cardName}'");
            }
        }

        ArrangeHand();
    }


    public void PlayCard(CardUI ui)
    {
        var card = ui.GetCard();
        if (hand.Contains(card))
        {
            hand.Remove(card);
            handUI.Remove(ui);
            Destroy(ui.gameObject);

            deckManager.Discard(card);
            ArrangeHand();
        }
    }

    private void ArrangeHand()
    {
        // Simple fan-out layout: spread cards across a width
        float totalWidth = 300f;
        float spacing = totalWidth / Mathf.Max(1, handUI.Count - 1);
        for (int i = 0; i < handUI.Count; i++)
        {
            Vector3 target = new Vector3(-totalWidth / 2 + spacing * i, 0, 0);
            handUI[i].AnimateToPosition(target, Quaternion.identity);
        }
    }

    public void DiscardHand()
    {
        foreach (var ui in handUI)
            Destroy(ui.gameObject);
        foreach (var card in hand)
            deckManager.Discard(card);

        hand.Clear();
        handUI.Clear();
    }
}
