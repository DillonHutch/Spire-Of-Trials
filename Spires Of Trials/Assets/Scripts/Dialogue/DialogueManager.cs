using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using FMODUnity;
using FMOD.Studio;



public class DialogueManager : MonoBehaviour
{

    [Header("Ink Story")]

    [SerializeField] private TextAsset inkJson;

    private Story story;

    private int currentChoiceIndex = -1;

    private bool dialoguePlaying = false;

    private InkExternalFunctions inkExternalFunctions;

    private InkDialogueVariables inkDialogueVariables;

    private void Awake()
    {
        story = new Story(inkJson.text);

        inkExternalFunctions = new InkExternalFunctions();

        inkExternalFunctions.Bind(story);

        inkDialogueVariables = new InkDialogueVariables(story);
    }

    private void OnDestroy()
    {
        inkExternalFunctions.Unbind(story);
    }


    private void OnEnable()
    {
        EventManager.Instance.StartListening<string>("enterDialogue", EnterDialogue);
        EventManager.Instance.StartListening<InputEventContext>("submitPressed", SubmitPressed);
        EventManager.Instance.StartListening<int>("updateChoiceIndex", UpdateChoiceIndex);
        EventManager.Instance.StartListening<(string, Ink.Runtime.Object)>(
                                                              "updateInkDialogueVariable",
                                                              data => UpdateInkDialogueVariable(data.Item1, data.Item2)
                                                            );
        EventManager.Instance.StartListening<Quest>("questStateChange", QuestStateChange);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<string>("enterDialogue", EnterDialogue);
        EventManager.Instance.StopListening<InputEventContext>("submitPressed", SubmitPressed);
        EventManager.Instance.StopListening<int>("updateChoiceIndex", UpdateChoiceIndex);
        EventManager.Instance.StopListening<(string, Ink.Runtime.Object)>(
                                                      "updateInkDialogueVariable",
                                                      data => UpdateInkDialogueVariable(data.Item1, data.Item2)
                                                    );
        EventManager.Instance.StopListening<Quest>("questStateChange", QuestStateChange);
    }

    private void QuestStateChange(Quest quest)
    {
            EventManager.Instance.TriggerEvent(
                  "updateInkDialogueVariable",
                  (quest.info.id + "State", (Ink.Runtime.Object)new StringValue(quest.state.ToString()))
                            );
    }

    private void UpdateInkDialogueVariable(string name, Ink.Runtime.Object value)
    {
        inkDialogueVariables.UpdateVariableState(name, value);
       
    }

    private void UpdateChoiceIndex(int choiceIndex)
    {
        this.currentChoiceIndex = choiceIndex;
    }



    private void SubmitPressed(InputEventContext inputeventContext)
    {

        if (!inputeventContext.Equals(InputEventContext.DIALOGUE)) return;
        ContinueOrExitStory();
    }



    private void EnterDialogue(string knotName)
    {

        if (dialoguePlaying)
        {
            return;
        }
        dialoguePlaying = true;

        EventManager.Instance.TriggerEvent("dialogueStarted");

        EventManager.Instance.TriggerEvent("StopPlayerMovement");

        EventManager.Instance.ChangeInputEventContext(InputEventContext.DIALOGUE);

        if (!knotName.Equals(""))
        {
            story.ChoosePathString(knotName);
        }
        else
        {
            Debug.LogWarning("knot name was the empty string when entering dialogue.");
        }

        inkDialogueVariables.SyncVariablesAndStartListening(story);



        ContinueOrExitStory();




    }

    private void ContinueOrExitStory()
    {

        if(story.currentChoices.Count > 0 && currentChoiceIndex != -1)
        {
            story.ChooseChoiceIndex(currentChoiceIndex);

            currentChoiceIndex = -1;
        }


        if (story.canContinue)
        {
            string dialogueLine = story.Continue();


            while (IsLineBlank(dialogueLine) && story.canContinue)
            {
                dialogueLine = story.Continue();
            }

            if(IsLineBlank(dialogueLine) && !story.canContinue)
            {
                ExitDialogue();
            }
            else
            {
                EventManager.Instance.TriggerEvent("displayDialogue", (dialogueLine, story.currentChoices));
            }

   

            
        }
        else if(story.currentChoices.Count == 0)
        {
            ExitDialogue();
        }
    }

    private void ExitDialogue()
    {
       

        dialoguePlaying = false;

        EventManager.Instance.TriggerEvent("dialogueFinished");

        EventManager.Instance.TriggerEvent("StartPlayerMovement");

        EventManager.Instance.ChangeInputEventContext(InputEventContext.DEFAULT);

        inkDialogueVariables.StopListening(story);

        story.ResetState();
    }


    private bool IsLineBlank(string dialogueLine)
    {
        return dialogueLine.Trim().Equals("") || dialogueLine.Trim().Equals("\n");
    }

}
