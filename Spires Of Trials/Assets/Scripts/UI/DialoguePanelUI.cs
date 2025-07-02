using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialoguePanelUI : MonoBehaviour
{

    [Header("Components")]
    [SerializeField] private GameObject contentParent;

    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private DialogueChoiceButton[] choiceButtons;

    private void Awake()
    {
        contentParent.SetActive(false);
        ResetPanel();
    }

    private void OnEnable()
    {
        EventManager.Instance.StartListening("dialogueStarted", DialogueStarted);
        EventManager.Instance.StartListening("dialogueFinished", DialogueFinished);
        EventManager.Instance.StartListening<(string dialogueLine, List<Choice> dialogueChoices)>("displayDialogue",  data 
            => DisplayDialogue(data.dialogueLine, data.dialogueChoices));


}

    private void OnDisable()
    {
        EventManager.Instance.StopListening("dialogueStarted", DialogueStarted);
        EventManager.Instance.StopListening("dialogueFinished", DialogueFinished);
        EventManager.Instance.StopListening<(string dialogueLine, List<Choice> dialogueChoices)>("displayDialogue", data
            => DisplayDialogue(data.dialogueLine, data.dialogueChoices));
    }

    private void DialogueStarted()
    {
        contentParent.SetActive(true);
    }


    private void DialogueFinished()
    {
        contentParent.SetActive(false); 


        ResetPanel();
    }

    private void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices)
    {
        dialogueText.text = dialogueLine;



        if (dialogueChoices.Count > choiceButtons.Length)
        {
            Debug.LogError("More dialogue choices (" +
                dialogueChoices.Count + ") came through than are supported ("
                + choiceButtons.Length + ").");


        }

            foreach(DialogueChoiceButton choiceButton in choiceButtons)
            {
                choiceButton.gameObject.SetActive(false);
            }


            int choiceButtonIndex = dialogueChoices.Count - 1;
            for(int inkChoiceIndex = 0; inkChoiceIndex < dialogueChoices.Count; inkChoiceIndex++)
            {
                Choice dialogueChoice = dialogueChoices[inkChoiceIndex];
                DialogueChoiceButton choiceButton = choiceButtons[inkChoiceIndex];

                choiceButton.gameObject.SetActive(true);    
                choiceButton.SetChoiceText(dialogueChoice.text);
                choiceButton.SetChoiceIndex(inkChoiceIndex);

                if(inkChoiceIndex == 0)
                {
                    choiceButton.SelectButton();
                    EventManager.Instance.TriggerEvent("updateChoiceIndex", 0);
                }


                
            }


        
    }

    private void ResetPanel()
    {
        dialogueText.text = "";
    }


}
