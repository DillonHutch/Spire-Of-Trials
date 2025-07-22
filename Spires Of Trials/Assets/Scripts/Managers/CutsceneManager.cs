using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{




    private void OnEnable()
    {
        EventManager.Instance.StartListening<string, string>("moveNPCSequence", MoveNPCSequence);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<string, string>("moveNPCSequence", MoveNPCSequence);
    }



    private void MoveNPCSequence(string npcName, string sequence)
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
