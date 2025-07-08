using Ink.Parsed;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BadNPCScript : MonoBehaviour
{

    [SerializeField] BattleSceneController battleController;
    [Header("Which Ink file to play when I get touched?")]
    [SerializeField] private TextAsset inkJSON;

    private void Start()
    {
        if (battleController == null)
            battleController = FindObjectOfType<BattleSceneController>();

        // Make sure this NPC disappears when battle starts
        battleController.objectsToDisable.Add(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("OverworldPlayer"))
            return;

        // stash the JSON so the dialogue runner can pick it up
        BattleContext.PendingInkJSON = inkJSON;
    }



}
