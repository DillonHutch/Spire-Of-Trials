using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{

    private bool isFinished = false;

    private string questId;

    private int stepIndex;

    public void InitializeQuestStep(string questId, int stepIndex, string questSteptState)
    {
        this.questId = questId;
        this.stepIndex = stepIndex;
        if(questSteptState != null && questSteptState != "")
        {
            SetQuestStepState(questSteptState);
        }
    }

    protected void FinishQuestStep()
    {

        if(!isFinished)
        {
            isFinished = true;

            EventManager.Instance.TriggerEvent("advanceQuest", questId);
            Destroy(this.gameObject);
        }


    }

    protected void ChangeState(string newState, string newStatus)
    {
        EventManager.Instance.TriggerEvent("questStepStateChange", (questId
            , stepIndex
            , new QuestStepState(newState, newStatus)));
    }

    protected abstract void SetQuestStepState(string state);

}
