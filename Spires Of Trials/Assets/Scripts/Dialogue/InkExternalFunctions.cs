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


        story.BindExternalFunction(
    "StartCombat",
    (string enemyTag) => StartCombat(enemyTag)
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

    private void StartCombat(string enemyTag)
    {
        // set whatever BattleContext info you need
        BattleContext.PendingEnemyTag = enemyTag;

        if(enemyTag != "Knight")
        {
            BattleContext.PendingEnemySlotCount = Random.Range(1, 4);
        }
           
        
        // now kick off the battle
        BattleSceneController bc = UnityEngine.Object.FindObjectOfType<BattleSceneController>();
        if (bc != null)
            bc.StartBattle();
        else
            Debug.LogError("No BattleSceneController in scene to start combat");
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
