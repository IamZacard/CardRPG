using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum TurnState { PlayerTurn, EnemyTurn }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    public TurnState currentTurn;
    public int playerAP;
    public int maxAP = 3;
    public Player player; // Assign in Inspector
    public Enemy enemy; // Assign in Inspector or find dynamically
    public List<EnemyData> possibleEnemies; // Assign in Inspector (Rat, Ogre King, etc.)
    public TMP_Text apText;
    public Button endTurnButton;
    public TMP_Text statusText;

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
        enemy = FindObjectOfType<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("No Enemy found in Combat scene!");
            return;
        }

        if (possibleEnemies == null || possibleEnemies.Count == 0)
        {
            Debug.LogError("No possible enemies assigned in CombatManager!");
            return;
        }

        // Assign random EnemyData
        EnemyData randomEnemy = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
        enemy.enemyData = randomEnemy;
        enemy.name = randomEnemy.Name;
        Debug.Log($"Spawned enemy: {randomEnemy.Name}");

        if (randomEnemy.Sprite == null)
        {
            Debug.LogWarning($"Enemy {randomEnemy.Name} has no sprite assigned in EnemyData!");
        }

        // Call Initialize after assigning enemyData
        enemy.Initialize();

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        playerAP = maxAP;
        DeckManager.Instance.DrawInitialHand();
        UpdateUI();
    }

    public void PlayCard(Card card, Character target)
    {
        if (currentTurn != TurnState.PlayerTurn || playerAP < card.cost)
        {
            Debug.LogWarning($"Cannot play {card.cardName}: Turn = {currentTurn}, AP = {playerAP}/{card.cost}");
            return;
        }

        if (card.damage > 0) target.TakeDamage(card.damage);
        if (card.block > 0) player.AddBlock(card.block);
        // Add status effect logic if applicable

        playerAP -= card.cost;
        DeckManager.Instance.DiscardCard(card);
        UpdateUI();
        Debug.Log($"Played {card.cardName}");
    }

    public void EndPlayerTurn()
    {
        Debug.Log("Player turn ended");
        while (DeckManager.Instance.Hand.Count > 0)
            DeckManager.Instance.DiscardCard(DeckManager.Instance.Hand[0]);
        currentTurn = TurnState.EnemyTurn;
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        if (enemy != null)
        {
            enemy.PerformAction(player);
            EndEnemyTurn();
        }
        else
        {
            OnEnemyDeath(null);
        }
    }

    void EndEnemyTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        StartPlayerTurn();
    }

    public void OnEnemyDeath(Character enemy)
    {
        Debug.Log("Combat won! Returning to Map scene.");
        SceneManager.LoadScene("Map");
    }

    void UpdateUI()
    {
        if (apText != null) apText.text = $"AP: {playerAP}";
        if (statusText != null) statusText.text = currentTurn == TurnState.PlayerTurn ? "Player Turn" : "Enemy Turn";
        if (endTurnButton != null)
        {
            endTurnButton.interactable = currentTurn == TurnState.PlayerTurn;
            var colors = endTurnButton.colors;
            colors.normalColor = playerAP > 0 ? Color.yellow : Color.green;
            endTurnButton.colors = colors;
        }
    }
}