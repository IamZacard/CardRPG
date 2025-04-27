using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandUIManager : MonoBehaviour
{
    public GameObject cardUIPrefab;
    public Transform handPanel;
    private List<GameObject> cardUIs = new List<GameObject>();

    public void UpdateHand(List<Card> hand)
    {
        foreach (var cardUI in cardUIs) Destroy(cardUI);
        cardUIs.Clear();

        foreach (var card in hand)
        {
            GameObject newCardUI = Instantiate(cardUIPrefab, handPanel);
            CardUI cardUIScript = newCardUI.GetComponent<CardUI>();
            cardUIScript.Setup(card);
            Button cardButton = newCardUI.GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(() => PlayCard(card));
            }
            cardUIs.Add(newCardUI);
        }
    }

    private void PlayCard(Card card)
    {
        if (CombatManager.Instance != null)
        {
            Character target = FindObjectOfType<Enemy>();
            if (target != null)
                CombatManager.Instance.PlayCard(card, target);
            else
                Debug.LogWarning("No target (Enemy or Boss) found for card play!");
        }
        else
        {
            Debug.LogWarning("CombatManager.Instance is null!");
        }
    }
}