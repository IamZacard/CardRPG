using UnityEngine;

[CreateAssetMenu(fileName = "CardTypeIcons", menuName = "Cards/Card Type Icon Set")]
public class CardTypeIconSet : ScriptableObject
{
    public Sprite attackIcon;
    public Sprite skillIcon;
    public Sprite powerIcon;
    public Sprite statusIcon;
    public Sprite curseIcon;

    public Sprite GetIcon(CardType type)
    {
        return type switch
        {
            CardType.Attack => attackIcon,
            CardType.Skill => skillIcon,
            CardType.Power => powerIcon,
            CardType.Status => statusIcon,
            CardType.Curse => curseIcon,
            _ => null
        };
    }
}
