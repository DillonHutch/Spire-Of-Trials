using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTriggerBattle : MonoBehaviour
{

    void Start()
    {
        // if the Overworld/EnemySpawner set a pending JSON, use it
        var ink = BattleContext.PendingInkJSON;
        if (ink != null)
        {
            BattleDialogueManager.GetInstance().EnterDialogueMode(ink);
            BattleContext.PendingInkJSON = null;  // clear it so you don’t replay it by accident
            return;
        }

        // fallback if none was set
        Debug.LogWarning("No pending Ink JSON; falling back to default");

    }


}
