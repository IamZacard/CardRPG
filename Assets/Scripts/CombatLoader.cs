using UnityEngine;

public class CombatLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private PlayerData playerData;

    [Header("UI")]
    [SerializeField] private GameObject playerHUDPrefab;

    [Header("Card Systems")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private HandManager handManager;

    private void Start()
    {
        // 1) Spawn player
        GameObject pGo = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        Player player = pGo.GetComponent<Player>();
        player.SetPlayerData(playerData);

        // 2) Spawn and initialize HUD
        GameObject hudGo = Instantiate(playerHUDPrefab);
        var hud = hudGo.GetComponent<PlayerHUDController>();
        hud.Initialize(player);

        // 3) Init card systems
        deckManager.InitializeDeck();
        handManager.DrawToHand(5);
    }
}
