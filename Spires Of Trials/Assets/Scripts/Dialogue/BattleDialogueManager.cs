using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using FMODUnity;
using FMOD.Studio;

public class BattleDialogueManager : MonoBehaviour
{

    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f;

    [SerializeField] private GameObject continueIcon;

    [Header("Globals Ink File")]

    [SerializeField] private TextAsset loadGlobalsJSON;

    [Header("Audio")]

    [SerializeField] private DialogueAudioInfoSO defaultAudioInfo;

    [SerializeField] private bool makePredictable;

    [SerializeField] private DialogueAudioInfoSO[] audioInfos;
    private Dictionary<string, DialogueAudioInfoSO> audioInfosDictionary;

    private DialogueAudioInfoSO currentAudioInfo;




    private static BattleDialogueManager instance;


    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private GameObject portraitFrame;
    [SerializeField] private Animator portraitAnimator;




    private Animator layoutAnimator;

    private Story currentStory;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;


    public bool dialogueIsPlaying { get; private set; }


    private Coroutine displayLineCoroutine;

    private const string SPEAKER_TAG = "speaker";

    private const string PORTRAIT_TAG = "portrait";

    private const string LAYOUT_TAG = "layout";

    private const string OBJECT_TAG = "object";

    private const string AUDIO_TAG = "audio";

    private DialogueVariables dialogueVariables;


    private bool canContinueToNextLine = false;

    private void Awake()
    {

        instance = this;

        if (instance == null)
        {
            Debug.LogWarning("Found multiple Dialouge Managers");
        }


        dialogueVariables = new DialogueVariables(loadGlobalsJSON);

        currentAudioInfo = defaultAudioInfo;

    }

    public static BattleDialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);


        layoutAnimator = dialoguePanel.GetComponent<Animator>();

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }

        InitializeAudioInfoDictionary();

        EventManager.Instance.StartListening("OnStopFight", OnBattleEnd);
    }

    private void OnBattleEnd()
    {
        if (dialogueIsPlaying)
        {
            ContinueStory();
        }
    }


    private void InitializeAudioInfoDictionary()
    {
        audioInfosDictionary = new Dictionary<string, DialogueAudioInfoSO>();
        audioInfosDictionary.Add(defaultAudioInfo.id, defaultAudioInfo);
        foreach (DialogueAudioInfoSO audioInfo in audioInfos)
        {
            audioInfosDictionary.Add(audioInfo.id, audioInfo);
        }
    }

    private void SetCurrentAudioInfo(string id)
    {
        DialogueAudioInfoSO audioInfo = null;
        audioInfosDictionary.TryGetValue(id, out audioInfo);
        if (audioInfo != null)
        {
            this.currentAudioInfo = audioInfo;
        }
        else
        {
            Debug.LogWarning("Failed to find audio info for id: " + id);
        }
    }


    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return;
        }

     

    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);


        dialogueVariables.StartListening(currentStory);


        displayNameText.text = "???";
        portraitAnimator.Play("default");
        layoutAnimator.Play("right");
        EventManager.Instance.TriggerEvent("setDialogueAudio", defaultAudioInfo.id);

        ContinueStory();

    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialogueVariables.StopListening(currentStory);

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";


        SetCurrentAudioInfo(defaultAudioInfo.id);
    }

    private void ContinueStory()
    {
        if (!currentStory.canContinue)
        {
            StartCoroutine(ExitDialogueMode());
            return;
        }

        // 1) stop any existing coroutine
        if (displayLineCoroutine != null)
            StopCoroutine(displayLineCoroutine);

        // 2) grab the next line
        string line = currentStory.Continue();

        // 3) apply speaker/portrait/layout/object tags *before* rendering
        HandleTags(currentStory.currentTags);

        // 4) now start typing it out
        displayLineCoroutine = StartCoroutine(DisplayLine(line));
    }



    private IEnumerator DisplayLine(string line)
    {


        // apply tags right *before* any text goes up
        HandleTags(currentStory.currentTags);

        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        continueIcon.SetActive(false);
        HideChoices();


        canContinueToNextLine = false;

        bool isAddingRichTextTag = false;


        foreach (char letter in line.ToCharArray())
        {

            if (Input.GetKey(KeyCode.Q))
            {
                dialogueText.maxVisibleCharacters = line.Length;
                break;
            }

            if (letter == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                //dialogueText.text += letter;
                if (letter == '>')
                {
                    isAddingRichTextTag = false;
                }
            }
            else
            {

                PlayDialogueSound(dialogueText.maxVisibleCharacters, dialogueText.text[dialogueText.maxVisibleCharacters]);
                dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(typingSpeed);
            }



        }


        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count == 0)
        {
            //continueIcon.SetActive(true);
        }


        DisplayChoices();

        canContinueToNextLine = true;
    }


    private void PlayDialogueSound(int currentDisplayedCharacterCount, char currentCharacter)
    {

        EventReference[] dialogueTypingSoundClips = currentAudioInfo.dialogueTypingSoundClips;
        int frequencyLevel = currentAudioInfo.frequencyLevel;
        float minPitch = currentAudioInfo.minPitch;
        float maxPitch = currentAudioInfo.maxPitch;
        bool stopAudioSource = currentAudioInfo.stopAudioSource;



        if (currentDisplayedCharacterCount % frequencyLevel == 0)
        {

            int randomIndex = Random.Range(0, dialogueTypingSoundClips.Length);


            EventInstance beepInstance = RuntimeManager.CreateInstance(dialogueTypingSoundClips[randomIndex]);


            if (stopAudioSource)
            {
                beepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }

            if (makePredictable)
            {
                int hashCode = currentCharacter.GetHashCode();

                int predictableIndex = hashCode % dialogueTypingSoundClips.Length;


                int minPitchInt = (int)(minPitch * 100);
                int maxPitchInt = (int)(maxPitch * 100);
                int pitchRangeInt = maxPitchInt - minPitchInt;


                if (pitchRangeInt != 0)
                {
                    int predictablePitchInt = (hashCode % pitchRangeInt) + minPitchInt;
                    float predictablePitch = predictablePitchInt / 100f;
                    beepInstance.setPitch(predictablePitch);
                }
                else
                {
                    beepInstance.setPitch(minPitch);
                }

            }

            else
            {


                // beepInstance.setPitch(Random.Range(minPitch, maxPitch));


            }
            beepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            beepInstance.start();
            beepInstance.release();

        }
    }

    private void HideChoices()
    {
        foreach (GameObject choiceButton in choices)
        {
            choiceButton.SetActive(false);
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
                case OBJECT_TAG:
                    bool isObject;
                    if (tagvalue == "true") { isObject = false; } else { isObject = true; }
                    portraitFrame.SetActive(isObject);
                    break;
                case AUDIO_TAG:
                    SetCurrentAudioInfo(tagvalue);
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                    break;
            }
        }
    }

    private void DisplayChoices()
    {

        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("more choices were given then the UI can support. Number of choices given: " + currentChoices.Count);
        }

        int index = 0;

        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }


        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());

    }


    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {

        if (canContinueToNextLine)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();
        }
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        dialogueVariables.variables.TryGetValue(variableName, out variableValue);
        if (variableValue != null)
        {
            Debug.LogWarning("Ink Variable was found to be null: " + variableName);
        }
        return variableValue;
    }

}
