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



    public void MoveNPCSequence(string npcName, string sequence)
    {
        GameObject go = GameObject.Find(npcName);
        if (go == null)
        {
            Debug.LogWarning($"MoveNPCSequence: no GameObject named \"{npcName}\"");
            return;
        }

        NPC mover = go.GetComponent<NPC>();
        if (mover == null)
        {
            Debug.LogWarning($"MoveNPCSequence: \"{npcName}\" has no NPC component");
            return;
        }

        List<MoveInstruction> moves = new List<MoveInstruction>();
        string[] segments = sequence.Split(',');
        foreach (string segment in segments)
        {
            string[] parts = segment.Split(':');
            if (parts.Length != 2)
                continue;

            string dirToken = parts[0].Trim().ToLowerInvariant();
            string distToken = parts[1].Trim();

            float distValue;
            if (!float.TryParse(distToken, out distValue))
                continue;

            Vector3 dir;
            switch (dirToken)
            {
                case "up":
                    dir = Vector3.up;
                    break;
                case "down":
                    dir = Vector3.down;
                    break;
                case "left":
                    dir = Vector3.left;
                    break;
                case "right":
                    dir = Vector3.right;
                    break;
                default:
                    dir = Vector3.zero;
                    break;
            }

            if (dir == Vector3.zero)
                continue;

            moves.Add(new MoveInstruction(dir, distValue));
        }

        IEnumerator sequenceCoroutine = mover.MoveSequence(
            moves,
            onComplete: () => EventManager.Instance.TriggerEvent("moveFinished", npcName)
        );
        mover.StartCoroutine(sequenceCoroutine);
    }



}
