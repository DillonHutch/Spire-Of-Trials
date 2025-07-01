using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestManager : MonoBehaviour
{

    [Header("Config")]
    [SerializeField] private bool loadQuestState = true;


    private Dictionary<string, Quest> questMap;


    private int currentPlayerLevel;



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

        EventManager.Instance.StartListening<(string id, int stepIndex, QuestStepState questStepState)>
            ("questStepStateChange", data => QuestStepStateChange(data.id, data.stepIndex, data.questStepState));


        EventManager.Instance.StartListening<int>("playerLevelChange", PlayerLevelChange);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<string>("startQuest", StartQuest);
        EventManager.Instance.StopListening<string>("advanceQuest", AdvanceQuest);
        EventManager.Instance.StopListening<string>("finishQuest", FinishQuest);


        EventManager.Instance.StopListening<(string id, int stepIndex, QuestStepState questStepState)>
            ("questStepStateChange", data => QuestStepStateChange(data.id, data.stepIndex, data.questStepState));

        EventManager.Instance.StopListening<int>("playerLevelChange", PlayerLevelChange);
    }


    private void Start()
    {
        foreach(Quest quest in questMap.Values)
        {

            if(quest.state == QuestState.IN_PROGRESS)
            {
                quest.InstatiateCurrentQuestStep(this.transform);
            }

            EventManager.Instance.TriggerEvent("questStateChange", quest);
        }
    }

    private void ChangeQuestState(string id, QuestState state)
    {
        Quest quest = GetQuestById(id);
        quest.state = state;
        EventManager.Instance.TriggerEvent("questStateChange", quest);
    }

    private void PlayerLevelChange(int level)
    {
        currentPlayerLevel = level;
    }

    private bool CheckRequirementsMet(Quest quest)
    {
        bool meetsRequirments = true;

        if(currentPlayerLevel < quest.info.levelRequirments)
        {
            meetsRequirments = false;
        }

        foreach(QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
        {
            meetsRequirments = false;
            break;

        }
        return meetsRequirments;
    }

    private void Update()
    {
        foreach(Quest quest in questMap.Values)
        {
            if(quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
            {
                ChangeQuestState(quest.info.id, QuestState.CAN_START);
            }
        }
    }


    private void StartQuest(string id)
    {
        Quest quest = GetQuestById(id);
        quest.InstatiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
    }

    private void AdvanceQuest(string id)
    {
        Quest quest = GetQuestById(id);


        quest.MoveToNextStep();

        if (quest.CurrentStepExists())
        {
            quest.InstatiateCurrentQuestStep(this.transform);

        }
        else
        {
            ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
        }
    }

    private void FinishQuest(string id)
    {
        Quest quest = GetQuestById(id);
        ClaimRewards(quest);
        ChangeQuestState(quest.info.id, QuestState.FINSIHED);
    }

    private void ClaimRewards(Quest quest)
    {
        EventManager.Instance.TriggerEvent("takeDamageEvent", 50);
        Debug.Log("Quest Done");
    }

    private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        Quest quest = GetQuestById(id);
        quest.StoreQuestStepSate(questStepState, stepIndex);
        ChangeQuestState(id, quest.state);
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

            idToQuestMap.Add(questInfo.id, LoadQuest(questInfo));
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

    private void OnApplicationQuit()
    {
        foreach(Quest quest in questMap.Values)
        {
            SaveQuest(quest);
        }
    }


    private void SaveQuest(Quest quest)
    {

        try
        {
            QuestData questData = quest.GetQuestData();
            string serializedData = JsonUtility.ToJson(questData);

            //THIS IS JUST TO GET THINGS WORKING NOT FOR LONG TERM!!!
            PlayerPrefs.SetString(quest.info.id, serializedData);

            
        }
        catch(System.Exception e)
        {
            Debug.LogError("Failed to save quest with id " + quest.info.id + ": " + e);
        }


    }

    private Quest LoadQuest(QuestInfoSO questInfo)
    {
        Quest quest = null;

        try
        {

            if (PlayerPrefs.HasKey(questInfo.id) && loadQuestState)
            {
                string serializedData = PlayerPrefs.GetString(questInfo.id);
                QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
            }
            else
            {
                quest = new Quest(questInfo);
            }

        }
        catch(System.Exception e)
        {
            Debug.LogError("Failed to load quest qith id " + quest.info.id + ": " + e);
        }


        return quest;
    }


}
