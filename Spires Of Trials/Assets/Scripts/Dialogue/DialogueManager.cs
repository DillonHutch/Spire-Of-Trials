using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using FMODUnity;
using FMOD.Studio;
using UnityEditor.Rendering;



public class DialogueManager : MonoBehaviour
{

    [Header("Ink Story")]

    [SerializeField] private TextAsset inkJson;

    private Story story;

    private int currentChoiceIndex = -1;

    private bool dialoguePlaying = false;

    private InkExternalFunctions inkExternalFunctions;

    private InkDialogueVariables inkDialogueVariables;


    private const string SPEAKER_TAG = "speaker";

    private const string PORTRAIT_TAG = "portrait";

    private const string LAYOUT_TAG = "layout";

    private const string AUDIO_TAG = "audio";


    private const string OBJECT_TAG = "object";


    private const string SPEED_TAG = "speed";

    private const string FREQUENCY_TAG = "frequency";




    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    //[SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private GameObject portraitFrame;
    [SerializeField] private Animator portraitAnimator;





    private Animator layoutAnimator;

    private bool canContinueToNextLine = false;

    private bool suppressResume = false;

    private void Awake()
    {
        story = new Story(inkJson.text);

        inkExternalFunctions = new InkExternalFunctions();

        inkExternalFunctions.Bind(story);

        inkDialogueVariables = new InkDialogueVariables(story);
    }

    private void Start()
    {
        layoutAnimator = dialoguePanel.GetComponent<Animator>();
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

        EventManager.Instance.StartListening("dialogueLineFinishedTyping", OnLineFinishedTyping);

        EventManager.Instance.StartListening("suppressDialogueResume", OnSuppressResume);
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

        EventManager.Instance.StopListening("dialogueLineFinishedTyping", OnLineFinishedTyping);

        EventManager.Instance.StopListening("suppressDialogueResume", OnSuppressResume);
    }

    private void OnSuppressResume()
    {
        suppressResume = true;
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


        if (!canContinueToNextLine)
            return;

        canContinueToNextLine = false;
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

        displayNameText.text = "???";
        portraitAnimator.Play("Default");
        layoutAnimator.Play("right");
        portraitFrame.gameObject.SetActive(true);
        



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
            HandleTags(story.currentTags);

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

                canContinueToNextLine = false;
                EventManager.Instance.TriggerEvent("displayDialogue", (dialogueLine, story.currentChoices));
            }

            


        }
        else if(story.currentChoices.Count == 0)
        {
            ExitDialogue();
        }
    }

   

    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
            }

            string tagkey = splitTag[0].Trim();
            string tagvalue = splitTag[1].Trim();



            switch (tagkey)
            {
                case SPEAKER_TAG:
                    displayNameText.text = tagvalue;
                    break;
                case PORTRAIT_TAG:
                    portraitAnimator.Play(tagvalue);
                    break;
                case LAYOUT_TAG:
                    layoutAnimator.Play(tagvalue);
                    break;
                case AUDIO_TAG:
                    EventManager.Instance.TriggerEvent("setDialogueAudio", tagvalue);
                    break;
                case OBJECT_TAG:
                    bool isObject;
                    if (tagvalue == "true") { isObject = false; } else { isObject = true; }
                    portraitFrame.SetActive(isObject);
                    break;
                case SPEED_TAG:
                    {
                        if (float.TryParse(tagvalue, out var newSpeed))
                        {
                            EventManager.Instance.TriggerEvent("setTypingSpeed", newSpeed);
                        }
                        else
                        {
                            Debug.LogError($"Invalid speed value: {tagvalue}");
                        }
                        break;
                    }
                case FREQUENCY_TAG:
                    if (int.TryParse(tagvalue, out var freq))
                        EventManager.Instance.TriggerEvent("setDialogueFrequency", freq);
                    else
                        Debug.LogError($"Invalid frequency value: {tagvalue}");
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                    break;
            }
        }
    }

    private void ExitDialogue()
    {
        dialoguePlaying = false;
        EventManager.Instance.TriggerEvent("dialogueFinished");

        // only resume movement/input if we were *not* told to suppress
        if (!suppressResume)
        {
            EventManager.Instance.TriggerEvent("StartPlayerMovement");
            EventManager.Instance.ChangeInputEventContext(InputEventContext.DEFAULT);
        }

        // reset for next time
        suppressResume = false;

        inkDialogueVariables.StopListening(story);
        story.ResetState();
    }


    private bool IsLineBlank(string dialogueLine)
    {
        return dialogueLine.Trim().Equals("") || dialogueLine.Trim().Equals("\n");
    }

    public void OnLineFinishedTyping()
    {
        canContinueToNextLine = true;
    }

}
