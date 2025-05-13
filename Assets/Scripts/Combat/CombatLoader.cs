using UnityEngine;
using System.Collections.Generic;

public class CombatLoader : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject playerHUDPrefab;

    [Header("Enemy Setup")]
    [SerializeField] private List<EnemyData> enemyDataList;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemyHUDPrefab;

    [Header("Card Systems")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private HandManager handManager;

    private void Start()
    {
        // — Spawn Player & HUD —
        var pGo = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        var player = pGo.GetComponent<Player>();
        player.SetPlayerData(playerData);

        var pHudGo = Instantiate(playerHUDPrefab);
        pHudGo.GetComponent<PlayerHUDController>().Initialize(player);

        // — Spawn Enemy & HUD —
        if (enemyDataList != null && enemyDataList.Count > 0)
        {
            var data = enemyDataList[Random.Range(0, enemyDataList.Count)];
            // Use factory to spawn at point
            Enemy enemy = EnemyFactory.CreateEnemy(data, enemySpawnPoint.position, enemySpawnPoint);

            // HUD
            var eHudGo = Instantiate(enemyHUDPrefab);
            eHudGo.GetComponent<EnemyHUDController>().Initialize(enemy);
        }
        else
        {
            Debug.LogWarning("CombatLoader: No EnemyData in list!");
        }

        // — Initialize Cards & Draw —
        deckManager.InitializeDeck();
        handManager.DrawToHand(5);
    }
}
