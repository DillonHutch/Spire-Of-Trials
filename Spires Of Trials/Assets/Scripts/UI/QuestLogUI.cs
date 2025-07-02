using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;


public class QuestLogUI : MonoBehaviour
{

    [Header("Components")]

    [SerializeField] private GameObject contentParent;

    [SerializeField] private QuestLogScrollingList scrollingList;

    [SerializeField] private TextMeshProUGUI questDisplayNameText;

    [SerializeField] private TextMeshProUGUI questStatusText;

    [SerializeField] private TextMeshProUGUI goldRewardsText;

    [SerializeField] private TextMeshProUGUI experienceRewardsText;

    [SerializeField] private TextMeshProUGUI levelRequirementsText;

    [SerializeField] private TextMeshProUGUI questRequirementsText;

    private Button firstSlectedButton;


    private void OnEnable()
    {
        EventManager.Instance.StartListening<Quest>("questStateChange", QuestStateChange);
    }


    private void OnDisable()
    {
        EventManager.Instance.StopListening<Quest>("questStateChange", QuestStateChange);
    }


    private void QuestLogTogglePressed()
    {
        if (contentParent.activeInHierarchy)
        {
            HideUI();
        }
        else
        {
            ShowUI();
        }
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.I))
        {
            QuestLogTogglePressed();
        }
    }

    private void ShowUI()
    {
        contentParent.SetActive(true);
        if(firstSlectedButton != null)
        {
            firstSlectedButton.Select();
        }
        Time.timeScale = 0;
    }

    private void HideUI()
    {
        contentParent.SetActive(false);
        Time.timeScale = 1;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void QuestStateChange(Quest quest)
    {
        QuestLogButton questLogButton = scrollingList.CreateButtonIfNotExists(quest, () =>
        {
            SetQuestLogInfo(quest);
        });

        if(firstSlectedButton ==  null)
        {
            firstSlectedButton = questLogButton.button;
           
        }


        questLogButton.SetState(quest.state);

    }

    private void SetQuestLogInfo(Quest quest)
    {

        questDisplayNameText.text = quest.info.displayName;

        questStatusText.text = quest.GetFullStatusText();


        levelRequirementsText.text = "Level " + quest.info.levelRequirments;
        questRequirementsText.text = "";
        foreach(QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
        {
            questRequirementsText.text += prerequisiteQuestInfo.displayName + "\n";
        }

        goldRewardsText.text = quest.info.timeTaken + " seconds of TIME";
        experienceRewardsText.text = quest.info.experienceReward + " XP";
    }


}
