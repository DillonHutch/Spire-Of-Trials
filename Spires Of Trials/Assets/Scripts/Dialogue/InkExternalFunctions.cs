using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkExternalFunctions 
{

    private string _pendingPostCombatKnot;

    public void Bind(Story story)
    {
        story.BindExternalFunction("StartQuest", (string questId) => StartQuest(questId));
        story.BindExternalFunction("AdvanceQuest", (string questId) => AdvanceQuest(questId));
        story.BindExternalFunction("FinishQuest", (string questId) => FinishQuest(questId));

        story.BindExternalFunction(
            "MoveNPCSequence",
            (string npcName, string seq) => MoveNPCSequence(npcName, seq)
        );


        story.BindExternalFunction(
        "StartCombat",
        (string enemyTag, string postCombatKnot) => StartCombat(enemyTag, postCombatKnot)
      );

    }

    public void Unbind(Story story)
    {
        story.UnbindExternalFunction("StartQuest");
        story.UnbindExternalFunction("AdvanceQuest");
        story.UnbindExternalFunction("FinishQuest");
        story.UnbindExternalFunction("MoveNPCSequence");
        story.UnbindExternalFunction("StartCombat");
    }

    private void StartCombat(string enemyTag, string postCombatKnot)
    {

        EventManager.Instance.TriggerEvent("suppressDialogueResume");

        // remember which enemy and knot to return to
        BattleContext.PendingEnemyTag = enemyTag;
        _pendingPostCombatKnot = postCombatKnot;

        // tell the battle controller to suppress its automatic resume
        var bc = UnityEngine.Object.FindObjectOfType<BattleSceneController>();
        if (bc != null)
        {
            bc.SuppressResume = true;
            bc.StartBattle();
        }
        else
        {
            Debug.LogError("No BattleSceneController in scene");
        }

        // when the battle unloads, we’ll handle coming back into Ink
        EventManager.Instance.StartListening("battleSceneUnLoaded", OnCombatEnded);
    }


    private void OnCombatEnded()
    {

        EventManager.Instance.TriggerEvent("StopPlayerMovement");
        EventManager.Instance.StopListening("battleSceneUnLoaded", OnCombatEnded);

        if (!string.IsNullOrEmpty(_pendingPostCombatKnot))
        {
            // swap in the same Ink JSON you used for overworld
            
            EventManager.Instance.TriggerEvent("enterDialogue", _pendingPostCombatKnot);
            
            _pendingPostCombatKnot = null;
        }
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


    private void MoveNPCSequence(string npcName, string sequence)
    {
        EventManager.Instance.TriggerEvent("moveNPCSequence", npcName, sequence); 
    }





}
