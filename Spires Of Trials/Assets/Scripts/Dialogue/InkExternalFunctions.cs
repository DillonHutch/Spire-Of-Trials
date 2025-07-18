using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkExternalFunctions 
{

    public void Bind(Story story)
    {
        story.BindExternalFunction("StartQuest", (string questId) => StartQuest(questId));
        story.BindExternalFunction("AdvanceQuest", (string questId) => AdvanceQuest(questId));
        story.BindExternalFunction("FinishQuest", (string questId) => FinishQuest(questId));

        story.BindExternalFunction(
            "MoveNPCSequence",
            (string npcName, string seq) => MoveNPCSequence(npcName, seq)
        );

    }

    public void Unbind(Story story)
    {
        story.UnbindExternalFunction("StartQuest");
        story.UnbindExternalFunction("AdvanceQuest");
        story.UnbindExternalFunction("FinishQuest");
        story.UnbindExternalFunction("MoveNPCSequence");
    }


    private void StartQuest(string questId)
    {
        EventManager.Instance.TriggerEvent("startQuest", questId);
    }

    private void AdvanceQuest(string questId)
    {
        EventManager.Instance.TriggerEvent("advanceQuest", questId);
    }

    private void FinishQuest(string questId)
    {
        EventManager.Instance.TriggerEvent("finishQuest", questId);
    }


    //public void MoveNPC(string npcName, string direction, float distance)
    //{
    //    var go = GameObject.Find(npcName);
    //    if (go == null)
    //    {
    //        Debug.LogWarning($"MoveNPC: no GameObject named “{npcName}”");
    //        return;
    //    }

    //    var mover = go.GetComponent<NPC>();
    //    if (mover == null)
    //    {
    //        Debug.LogWarning($"MoveNPC: “{npcName}” has no NPC component");
    //        return;
    //    }

    //    Vector3 dir;
    //    switch (direction.ToLowerInvariant())
    //    {
    //        case "up": dir = Vector3.up; break;
    //        case "down": dir = Vector3.down; break;
    //        case "left": dir = Vector3.left; break;
    //        case "right": dir = Vector3.right; break;
    //        default:
    //            Debug.LogWarning($"MoveNPC: unknown direction “{direction}”");
    //            return;
    //    }

    //    // start the move coroutine, with a callback
    //    mover.StartCoroutine(
    //        mover.Move(dir, distance, 1f, onComplete: () =>
    //        {
    //            EventManager.Instance.TriggerEvent("moveFinished", npcName);
    //        })
    //    );
    //}


    public void MoveNPCSequence(string npcName, string sequence)
    {
        var go = GameObject.Find(npcName);
        if (go == null)
        {
            Debug.LogWarning($"MoveNPCSequence: no GameObject named “{npcName}”");
            return;
        }

        var mover = go.GetComponent<NPC>();
        if (mover == null)
        {
            Debug.LogWarning($"MoveNPCSequence: “{npcName}” has no NPC component");
            return;
        }

        // parse "right:1,left:2,up:3" into instructions
        var moves = new List<MoveInstruction>();
        foreach (var segment in sequence.Split(','))
        {
            var parts = segment.Split(':');
            if (parts.Length != 2) continue;
            var dirToken = parts[0].Trim().ToLowerInvariant();
            var distToken = parts[1].Trim();
            if (!float.TryParse(distToken, out var dist))
                continue;

            Vector3 dir = dirToken switch
            {
                "up" => Vector3.up,
                "down" => Vector3.down,
                "left" => Vector3.left,
                "right" => Vector3.right,
                _ => Vector3.zero
            };
            if (dir == Vector3.zero) continue;
            moves.Add(new MoveInstruction(dir, dist));
        }

        // run them in order, then fire the finish event
        mover.StartCoroutine(
            mover.MoveSequence(
                moves,
                onComplete: () => EventManager.Instance.TriggerEvent("moveFinished", npcName)
            )
        );
    }


}
