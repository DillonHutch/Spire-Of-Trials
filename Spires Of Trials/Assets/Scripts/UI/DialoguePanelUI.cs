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


    [Header("Type-writer Settings")]
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] private GameObject continueIcon;      // optional little arrow
    private Coroutine typingCoroutine;

  

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
        // stop any previous typing
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeDialogue(dialogueLine, dialogueChoices));
    }


    private IEnumerator TypeDialogue(string line, List<Choice> dialogueChoices)
    {
        // prepare the text
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        continueIcon.SetActive(false);
        HideAllChoices();

        bool isAddingRichTextTag = false;
        int totalVisible = 0;

        foreach (char c in line)
        {
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    dialogueText.maxVisibleCharacters = line.Length;
            //    EventManager.Instance.TriggerEvent("dialogueLineFinishedTyping");
            //    break;
            //}

            if (c == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                if (c == '>')
                    isAddingRichTextTag = false;
            }
            else
            {
                dialogueText.maxVisibleCharacters = ++totalVisible;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // all text is now visible
        dialogueText.maxVisibleCharacters = line.Length;

        // only show the continue-arrow if there are no choices
        if (dialogueChoices.Count == 0)
            continueIcon.SetActive(true);
        else
            continueIcon.SetActive(false);

        // now show choices (if any)
        ShowChoices(dialogueChoices);

        EventManager.Instance.TriggerEvent("dialogueLineFinishedTyping");

        typingCoroutine = null;
    }

    private void HideAllChoices()
    {
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    private void ShowChoices(List<Choice> dialogueChoices)
    {
        if (dialogueChoices.Count > choiceButtons.Length)
            Debug.LogError($"Too many choices: {dialogueChoices.Count}");

        for (int i = 0; i < dialogueChoices.Count; i++)
        {
            var btn = choiceButtons[i];
            btn.gameObject.SetActive(true);
            btn.SetChoiceText(dialogueChoices[i].text);
            btn.SetChoiceIndex(i);

            if (i == 0)
            {
                btn.SelectButton();
                EventManager.Instance.TriggerEvent("updateChoiceIndex", 0);
            }
        }
    }




    private void ResetPanel()
    {
        dialogueText.text = "";
    }


}
