using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(CircleCollider2D))]
public class QuestPoint : MonoBehaviour
{

    [Header("Quest")]
    [SerializeField] private QuestInfoSO questInfoForPoint;


    private bool playerIsNear = false;
    private string questId;
    private QuestState currentQuestState;


    private void Awake()
    {
        questId = questInfoForPoint.id;
    }

    private void OnEnable()
    {
        EventManager.Instance.StartListening<Quest>("questStateChange", QuestStateChange);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<Quest>("questStateChange", QuestStateChange);
    }

    private void SubmitPressed()
    {

        if(Input.GetKeyUp(KeyCode.Z))
        {
            if (!playerIsNear)
            {
                return;
            }

            EventManager.Instance.TriggerEvent("startQuest", questId);
            EventManager.Instance.TriggerEvent("advanceQuest", questId);
            EventManager.Instance.TriggerEvent("finishQuest", questId);
        }


    }

    private void Update()
    {
        
    }


    private void QuestStateChange(Quest quest)
    {
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            Debug.Log("Quest with id: " + questId + " updated to state: " + currentQuestState);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("OverworldPlayer"))
        {
            playerIsNear = true;    
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("OverworldPlayer"))
        {
            playerIsNear = false;
        }
    }




}
