// BadNPCScript.cs
using System.Collections.Generic;
using UnityEngine;

public class BadNPCScript : MonoBehaviour
{
    [Header("Which Ink file to play when I get touched?")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private List<string> enemyTagsList;

    [Header("Spawn Count Settings")]
    [SerializeField] private bool hasMultipleSpawns = false;

    [Header("Quip Settings")]
    [Tooltip("Chance that an enemy will quip at end of round (0–1)")]
    [SerializeField, Range(0f, 1f)] private float quipChance = 0.2f;

    private BattleSceneController battleController;

    private void Start()
    {
        battleController = battleController ?? FindObjectOfType<BattleSceneController>();
        battleController.objectsToDisable.Add(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("OverworldPlayer")) return;

        BattleContext.PendingEnemyTags = new List<string>(enemyTagsList);
        BattleContext.PendingInkJSON = inkJSON;
        BattleContext.PendingHasMultipleSpawns = hasMultipleSpawns;

        // ← push the NPC’s desired quip‐chance into the context
        BattleContext.PendingQuipChance = quipChance;
    }
}
