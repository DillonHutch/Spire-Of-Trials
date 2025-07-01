using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(CircleCollider2D))]
public class QuestPoint : MonoBehaviour
{

    [Header("Dialogue (optional)")]
    [SerializeField] private string dialogueKnotName;

    [Header("Quest")]
    [SerializeField] private QuestInfoSO questInfoForPoint;

    [Header("Config")]

    [SerializeField] private bool startpoint = true;
    [SerializeField] private bool finishPoint = true;   


    private bool playerIsNear = false;
    private string questId;
    private QuestState currentQuestState;
    private QuestIcon questIcon;


    private void Awake()
    {
        questId = questInfoForPoint.id;
        questIcon = GetComponentInChildren<QuestIcon>();    
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

            if (!dialogueKnotName.Equals(""))
            {
                EventManager.Instance.TriggerEvent("enterDialogue", dialogueKnotName);
            }
            else
            {
                if (currentQuestState.Equals(QuestState.CAN_START) && startpoint)
                {
                    EventManager.Instance.TriggerEvent("startQuest", questId);
                }
                else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
                {
                    EventManager.Instance.TriggerEvent("finishQuest", questId);
                }
            }
        }
    }

    private void Update()
    {
        SubmitPressed();
    }


    private void QuestStateChange(Quest quest)
    {
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            questIcon.SetState(currentQuestState, startpoint, finishPoint);
            
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
