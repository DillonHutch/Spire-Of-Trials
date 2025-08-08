using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.Linq;

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
        (InkList enemyTags, bool hasMultipleSpawns, float quipChance, string postCombatKnot)
          => StartCombat(enemyTags, hasMultipleSpawns, quipChance, postCombatKnot)
                          );


        story.BindExternalFunction("FocusCam", (string targetName) => FocusCam(targetName));

        story.BindExternalFunction("ResetCamera", () => ResetCamera());

    }




    public void Unbind(Story story)
    {
        story.UnbindExternalFunction("StartQuest");
        story.UnbindExternalFunction("AdvanceQuest");
        story.UnbindExternalFunction("FinishQuest");
        story.UnbindExternalFunction("MoveNPCSequence");
        story.UnbindExternalFunction("StartCombat");
        story.UnbindExternalFunction("FocusCam");
        story.UnbindExternalFunction("ResetCamera");
    }



    private void FocusCam(string targetName)
    {
        // route it through your EventManager
        EventManager.Instance.TriggerEvent("focusCamera", targetName);
    }

    private void ResetCamera()
    {
        // route it through your EventManager
        EventManager.Instance.TriggerEvent("resetCamera");
    }

    private void StartCombat(InkList enemyTags, bool hasMultipleSpawns, float quipChance, string postCombatKnot)
    {
        // convert to string[] by accessing the Key of each KeyValuePair, which is of type InkListItem
        var tags = enemyTags.ToList().Select(e => e.Key.itemName).ToArray();
        // put into your BattleContext, etc.
        BattleContext.PendingEnemyTags = new List<string>(tags);
        BattleContext.PendingHasMultipleSpawns = hasMultipleSpawns;
        BattleContext.PendingQuipChance = quipChance;
        _pendingPostCombatKnot = postCombatKnot;

        // 3) suppress ink auto-resume and fire off your battle events
        EventManager.Instance.TriggerEvent("suppressDialogueResume");
        EventManager.Instance.TriggerEvent("battleSceneControllerSupress");
        EventManager.Instance.TriggerEvent("StartBattle");

        // 4) once the battle unloading event fires, we’ll pop back into Ink
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
