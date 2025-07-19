using UnityEngine;
using Ink.Runtime;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class MovementTrigger : MonoBehaviour
{
    [Header("What to move")]
    [SerializeField] private string npcName;

    [Header("Move Sequence (e.g. \"right:2,left:1,up:1\")")]
    [SerializeField] private string moveSequence;

    [Header("Cinemachine Cameras")]
    [Tooltip("The Virtual Camera that normally follows the player")]
    [SerializeField] private CinemachineVirtualCamera playerCam;
    [Tooltip("A dedicated Virtual Camera for this NPC cutscene")]
    [SerializeField] private CinemachineVirtualCamera npcCam;

    [Header("After move, play this knot")]
    [SerializeField] private string dialogueKnot;

    // priority offsets
    [Tooltip("How much higher NPC cam priority should go above PlayerCam")]
    [SerializeField] private int camBoost = 10;


    [SerializeField] private TextAsset inkJSON;




    private int _playerPriority;
    private int _npcDefaultPriority;
    private bool _triggered;

    private void Start()
    {
        // cache their starting priorities
        _playerPriority = playerCam.Priority;
        _npcDefaultPriority = npcCam.Priority;
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("OverworldPlayer")) return;
        _triggered = true;

        // 1) lock out player movement
        EventManager.Instance.TriggerEvent("StopPlayerMovement");

        // 2) bump NPC cam up so Brain will blend to it
        npcCam.Priority = _playerPriority + camBoost;

        // 3) tell the NPC cam to follow/look at the NPC
        var go = GameObject.Find(npcName);
        if (go != null)
        {
            npcCam.Follow = go.transform;
            BattleContext.PendingInkJSON = inkJSON;
            //npcCam.LookAt = go.transform;
        }
        else Debug.LogWarning($"MovementTrigger: no GameObject named {npcName}");

        // 4) start the move+event sequence
       

        EventManager.Instance.TriggerEvent("moveNPCSequence", npcName, moveSequence);
        EventManager.Instance.StartListening<string>("moveFinished", OnMoveFinished);
    }

    private void OnMoveFinished(string finishedNpc)
    {
        if (finishedNpc != npcName) return;
        EventManager.Instance.StopListening<string>("moveFinished", OnMoveFinished);

        // kick off your Ink dialogue knot
        EventManager.Instance.TriggerEvent("enterDialogue", dialogueKnot);

        // when dialogue wraps, we'll switch back
        EventManager.Instance.StartListening("dialogueFinished", OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        EventManager.Instance.StopListening("dialogueFinished", OnDialogueFinished);

        // reset NPC cam priority so Brain blends back
        npcCam.Priority = _npcDefaultPriority;

        // DialogueManager will fire "StartPlayerMovement" automatically,
        // so you don't need to un‑freeze the player here.
    }


    


}
