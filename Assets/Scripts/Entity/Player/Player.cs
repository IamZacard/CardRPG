using UnityEngine;

public class Player : Entity
{
    public PlayerData playerData;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    public void SetPlayerData(PlayerData data)
    {
        playerData = data;
        Initialize();
    }

    public override void Initialize()
    {
        if (playerData != null)
        {
            entityName = playerData.playerName;
            maxHealth = playerData.maxHealth;
            maxEnergy = playerData.maxEnergy;
            baseBlock = playerData.baseBlock;

            if (spriteRenderer != null && playerData.characterSprite != null)
                spriteRenderer.sprite = playerData.characterSprite;
        }

        base.Initialize();
        currentEnergy = maxEnergy;
    }
}
