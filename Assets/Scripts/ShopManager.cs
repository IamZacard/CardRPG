using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public List<Card> availableCards;
    public GameObject cardUIPrefab;
    public Transform shopPanel;
    public Button returnButton;
    public TMP_Text goldText;
    private int playerGold = 50;

    void Start()
    {
        if (goldText != null)
            goldText.text = $"Gold: {playerGold}";
        foreach (var card in availableCards)
        {
            GameObject cardUI = Instantiate(cardUIPrefab, shopPanel);
            CardUI cardUIScript = cardUI.GetComponent<CardUI>();
            cardUIScript.Setup(card);
            cardUI.GetComponent<Button>().onClick.RemoveAllListeners();
            cardUI.GetComponent<Button>().onClick.AddListener(() => BuyCard(card));
        }
        if (returnButton != null)
            returnButton.onClick.AddListener(() => SceneManager.LoadScene("Map"));
    }

    void BuyCard(Card card)
    {
        int cardCost = card.cost * 10;
        if (playerGold >= cardCost)
        {
            playerGold -= cardCost;
            DeckManager.Instance.startingDeck.Add(card);
            SaveLoadManager.Instance.AutoSave();
            if (goldText != null)
                goldText.text = $"Gold: {playerGold}";
            Debug.Log($"Bought {card.cardName}");
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }

    public int GetPlayerGold() => playerGold;
    public void SetPlayerGold(int gold) => playerGold = gold;
}