using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest 
{

    public QuestInfoSO info;

    public QuestState state;

    private int currentQuestStepIndex;

    private QuestStepState[] questStepStates;

    public Quest(QuestInfoSO questInfo)
    {
        this.info = questInfo;
        this.state = QuestState.REQUIREMENTS_NOT_MET;
        this.currentQuestStepIndex = 0;
        this.questStepStates = new QuestStepState[info.questStepPrefabs.Length];
        for (int i = 0; i < questStepStates.Length; i++)
        {
            questStepStates[i] = new QuestStepState();  
        }

    }

    public Quest(QuestInfoSO questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        this.info = questInfo;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;


        if(this.questStepStates.Length != this.info.questStepPrefabs.Length)
        {
            Debug.LogWarning("Quest Step Prefabs and Quest Step States are" +
                "of differnt lengths. This indicates something changed" +
                "With the QuestINfo and the saved data is now out of sync. " +
                "Reset your data = as this might caused issues. QuestID: " + this.info.id);
        }
    }



    public void MoveToNextStep()
    {
        currentQuestStepIndex++;
    }

    public bool CurrentStepExists()
    {
        return (currentQuestStepIndex < info.questStepPrefabs.Length);
    }


    public void InstatiateCurrentQuestStep(Transform parentTransform)
    {
        GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        if (questStepPrefab != null)
        {
           QuestStep questStep = Object.Instantiate<GameObject>(questStepPrefab, parentTransform)
                .GetComponent<QuestStep>();
            questStep.InitializeQuestStep(info.id, currentQuestStepIndex, questStepStates[currentQuestStepIndex].state);
        }
    }

    private GameObject GetCurrentQuestStepPrefab()
    {
        GameObject questStepPrefab = null;
        if(CurrentStepExists())
        {
            questStepPrefab = info.questStepPrefabs[currentQuestStepIndex];
        }
        else
        {
            Debug.LogWarning("Tried to get quest step prefab, but stepIndex was out of range indicating that "
                + "there's no current step: QuizId= " + info.id + ", stepIndex=" + currentQuestStepIndex);
        }

        return questStepPrefab;
    }


    public void StoreQuestStepSate(QuestStepState questStepState, int stepIndex)
    {
        if(stepIndex < questStepStates.Length)
        {
            questStepStates[stepIndex].state = questStepState.state;
            questStepStates[stepIndex].status = questStepState.status;  
        }
        else
        {
            Debug.LogWarning("Tried to access quest step data, but stepIndex was out of range: " + "Quest Id = " + info.id + ", Step Index = " + stepIndex);
        }
    }


    public QuestData GetQuestData()
    {
        return new QuestData(state, currentQuestStepIndex, questStepStates);
    }

    public string GetFullStatusText()
    {

        string fullStatus = "";

        if(state == QuestState.REQUIREMENTS_NOT_MET)
        {
            fullStatus = "Requirments are not yet met to start this quest.";
        }
        else if(state == QuestState.CAN_START)
        {
            fullStatus = "This quest can be started!";
        }
        else
        {
            for(int i = 0; i < currentQuestStepIndex; i++)
            {
                fullStatus += "<s>" + questStepStates[i].status + "</s>\n";
            }

            if (CurrentStepExists())
            {
                fullStatus += questStepStates[currentQuestStepIndex].status;
            }

            if(state == QuestState.FINSIHED)
            {
                fullStatus += "The quest has been completed!";
            }
        }

        return fullStatus;

    }

}
