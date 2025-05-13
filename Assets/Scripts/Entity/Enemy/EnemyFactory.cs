using UnityEngine;
using System.Collections.Generic;

public static class EnemyFactory
{
    public static Enemy CreateEnemy(EnemyData enemyData, Vector3 position, Transform parent = null)
    {
        GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Entities/BaseEnemy");
        if (enemyPrefab == null)
        {
            Debug.LogError("Base enemy prefab not found in Resources!");
            return null;
        }

        GameObject enemyObject = Object.Instantiate(enemyPrefab, parent);
        enemyObject.transform.position = position; // Set spawn position**
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy component not found on prefab!");
            return null;
        }

        enemy.entityName = enemyData.enemyName;
        enemy.maxHealth = enemyData.maxHealth;
        enemy.currentHealth = enemyData.maxHealth;
        enemy.baseBlock = enemyData.baseBlock;
        enemy.isElite = enemyData.isElite;
        enemy.isBoss = enemyData.isBoss;
        enemy.goldReward = enemyData.goldReward;
        enemy.experienceReward = enemyData.experienceReward;

        // Set visual appearance using the artwork sprite
        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && enemyData.artwork != null)
        {
            spriteRenderer.sprite = enemyData.artwork;
        }
        else if (enemyData.artwork != null)
        {
            // If no SpriteRenderer exists on the prefab, add one
            spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = enemyData.artwork;

            // Set sorting layer and order for proper rendering
            spriteRenderer.sortingLayerName = "Entities";
            spriteRenderer.sortingOrder = 10;
        }

        enemy.possibleActions.Clear();
        foreach (var action in enemyData.possibleActions)
        {
            enemy.possibleActions.Add(action);
        }

        EnemyAI ai = enemyObject.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.aggressiveThreshold = enemyData.aggressiveThreshold;
            ai.defensiveThreshold = enemyData.defensiveThreshold;
            ai.healThreshold = enemyData.healThreshold;
            ai.aggressiveActions = enemyData.possibleActions.FindAll(a => a.damage > 0);
            ai.defensiveActions = enemyData.possibleActions.FindAll(a => a.block > 0);
            ai.healActions = enemyData.possibleActions.FindAll(a =>
                a.statusEffectType.HasValue &&
                a.selfEffect &&
                (a.statusEffectType == StatusEffectType.Strength || a.damage == 0));
        }

        /*if (enemyData.isBoss && enemyData.bossPhases.Count > 0)
        {
            BossController bossController = enemyObject.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.phases.Clear();
                foreach (var phaseData in enemyData.bossPhases)
                {
                    BossController.BossPhase phase = new BossController.BossPhase
                    {
                        healthPercentageThreshold = phaseData.healthPercentageThreshold,
                        phaseTransitionAction = phaseData.phaseTransitionAction
                    };
                    phase.phaseActions.AddRange(phaseData.phaseActions);
                    bossController.phases.Add(phase);
                }
            }
        }*/

        enemy.Initialize();
        return enemy;
    }
}