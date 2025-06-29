using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private Dictionary<string, Quest> questMap;

    private void Awake()
    {
        questMap = CreateQuestMap();

        Quest quest = GetQuestById("CollectCoinsQuest");

    }

    private void OnEnable()
    {
        EventManager.Instance.StartListening<string>("startQuest", StartQuest);
        EventManager.Instance.StartListening<string>("advanceQuest", AdvanceQuest);
        EventManager.Instance.StartListening<string>("finishQuest", FinishQuest);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<string>("startQuest", StartQuest);
        EventManager.Instance.StopListening<string>("advanceQuest", AdvanceQuest);
        EventManager.Instance.StopListening<string>("finishQuest", FinishQuest);
    }


    private void Start()
    {
        foreach(Quest quest in questMap.Values)
        {
            EventManager.Instance.TriggerEvent("questStateChange", quest);
        }
    }


    private void StartQuest(string id)
    {
        Debug.Log("Start Quest: " + id);
    }

    private void AdvanceQuest(string id)
    {
        Debug.Log("Advance Quest: " + id);
    }

    private void FinishQuest(string id)
    {
        Debug.Log("Finish Quest: " + id);
    }


    private Dictionary<string, Quest> CreateQuestMap()
    {

        QuestInfoSO[] allQuests = Resources.LoadAll<QuestInfoSO>("Quests");


        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
        foreach(QuestInfoSO questInfo in allQuests)
        {
            if (idToQuestMap.ContainsKey(questInfo.id))
            {
                Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
            }

            idToQuestMap.Add(questInfo.id, new Quest(questInfo));
        }
        return idToQuestMap;
    }

    private Quest GetQuestById(string id)
    {

        Quest quest = questMap[id];
        if(quest == null)
        {
            Debug.LogError("ID not found in the Quest Map: " + id);
        }
        return quest;
    }


}
