using UnityEngine;
using TMPro;

public class CardUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text typeText; // New: Displays CardType
    public TMP_Text descriptionText; // New: Displays formatted description
    public Card cardData;

    public void Setup(Card card)
    {
        cardData = card;
        if (nameText != null) nameText.text = card.cardName;
        if (costText != null) costText.text = card.cost.ToString();
        if (typeText != null) typeText.text = card.type.ToString();
        if (descriptionText != null) descriptionText.text = card.GetFormattedDescription();
    }
}