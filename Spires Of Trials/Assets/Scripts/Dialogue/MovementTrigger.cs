using UnityEngine;
using Ink.Runtime;

public class MovementTrigger : MonoBehaviour
{
    [Header("What to move")]
    [SerializeField] private string npcName;

    [Header("Move Sequence (e.g. \"right:2,left:1,up:1\")")]
    [SerializeField] private string moveSequence;

    [Header("After move, play this knot")]
    [SerializeField] private string dialogueKnot;

    private bool _hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("OverworldPlayer")) return;
        _hasTriggered = true;

        var binder = new InkExternalFunctions();
        binder.MoveNPCSequence(npcName, moveSequence);

        EventManager.Instance.StartListening<string>("moveFinished", OnMoveFinished);
    }

    private void OnMoveFinished(string finishedNpcName)
    {
        if (finishedNpcName != npcName) return;
        EventManager.Instance.StopListening<string>("moveFinished", OnMoveFinished);
        EventManager.Instance.TriggerEvent("enterDialogue", dialogueKnot);
    }
}
