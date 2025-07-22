using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ink.Runtime;
using Cinemachine;

[Serializable]
public struct NPCMovementEntry
{
    [Tooltip("Drag in the NPC GameObject to move")]
    public GameObject npc;
    [Tooltip("Move Sequence, e.g. \"right:2,left:1,up:1\"")]
    public string moveSequence;
}

public class MovementTrigger : MonoBehaviour
{
    [Header("What to move")]
    [SerializeField]
    private List<NPCMovementEntry> movements;

    [Header("Cinemachine Cameras")]
    [SerializeField]
    private CinemachineVirtualCamera playerCam;
    [SerializeField]
    private CinemachineVirtualCamera npcCam;
    [Tooltip("How much above playerCam priority")]
    [SerializeField]
    private int camBoost = 10;

    [Header("Optional NPC Cam Focus Target")]
    [Tooltip("If set, npcCam will Follow (and LookAt) this object")]
    [SerializeField]
    private GameObject npcCamTarget;

    [Header("After move, play this knot")]
    [SerializeField]
    private string dialogueKnot;

    [SerializeField] private TextAsset inkJSON;

    // runtime fields
    private List<string> _pendingNPCNames;
    private int _playerPriority;
    private int _npcDefaultPriority;
    private bool _triggered;

    // cache original vcam targets
    private Transform _originalNpcCamFollow;


    private void Start()
    {
        _playerPriority = playerCam.Priority;
        _npcDefaultPriority = npcCam.Priority;

        // store whatever the npcCam was following/looking at
        _originalNpcCamFollow = npcCam.Follow;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !other.CompareTag("OverworldPlayer"))
            return;
        _triggered = true;

        if (inkJSON != null)
            BattleContext.PendingInkJSON = inkJSON;

        // 1) stop the player
        EventManager.Instance.TriggerEvent("StopPlayerMovement");

        // 2) bump to NPC cam
        npcCam.Priority = _playerPriority + camBoost;

        // 3) if you assigned a focus target, swap the vcam’s Follow/LookAt
        if (npcCamTarget != null)
        {
            npcCam.Follow = npcCamTarget.transform;

        }

        // 4) build list of names we expect to finish
        _pendingNPCNames = movements
            .Where(e => e.npc != null)
            .Select(e => e.npc.name)
            .ToList();

        // 5) fire off moves
        foreach (var entry in movements)
        {
            if (entry.npc == null)
            {
                Debug.LogWarning("MovementTrigger: an NPC slot is empty");
                continue;
            }
            EventManager.Instance.TriggerEvent(
                "moveNPCSequence",
                entry.npc.name,
                entry.moveSequence
            );
        }

        // 6) listen for each finish
        EventManager.Instance.StartListening<string>(
            "moveFinished",
            OnMoveFinished
        );
    }

    private void OnMoveFinished(string finishedNpcName)
    {
        if (!_pendingNPCNames.Contains(finishedNpcName))
            return;

        _pendingNPCNames.Remove(finishedNpcName);
        if (_pendingNPCNames.Count > 0)
            return;

        EventManager.Instance.StopListening<string>(
            "moveFinished",
            OnMoveFinished
        );

        // start dialogue
        EventManager.Instance.TriggerEvent("enterDialogue", dialogueKnot);
        EventManager.Instance.StartListening(
            "dialogueFinished",
            OnDialogueFinished
        );
    }

    private void OnDialogueFinished()
    {
        EventManager.Instance.StopListening(
            "dialogueFinished",
            OnDialogueFinished
        );

        // restore cam priority
        npcCam.Priority = _npcDefaultPriority;

        // restore original Follow/LookAt
        if (_originalNpcCamFollow != null)
            npcCam.Follow = _originalNpcCamFollow;

        // DialogueManager will automatically fire StartPlayerMovement
    }
}
