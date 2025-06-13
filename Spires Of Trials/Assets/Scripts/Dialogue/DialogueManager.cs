using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;


public class DialogueManager : MonoBehaviour
{

    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f;

    [SerializeField] private GameObject continueIcon;

    [Header("Globals Ink File")]

    [SerializeField] private TextAsset loadGlobalsJSON;


    private static DialogueManager instance;


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

    }

    public static DialogueManager GetInstance()
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
        foreach(GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return;
        }

        if (currentStory.currentChoices.Count == 0 && canContinueToNextLine && Input.GetKeyDown(KeyCode.E))
        {
            ContinueStory();
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

        ContinueStory();

    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialogueVariables.StopListening(currentStory);

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
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


        foreach(char letter in line.ToCharArray())
        {

            if (Input.GetKey(KeyCode.Q))
            {
                dialogueText.maxVisibleCharacters = line.Length;
                break;
            }

            if(letter == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                //dialogueText.text += letter;
                if(letter == '>')
                {
                    isAddingRichTextTag = false;
                }
            }
            else
            {
                dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(typingSpeed);
            }



        }


        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count == 0)
        {
            continueIcon.SetActive(true);
        }

        
        DisplayChoices();

        canContinueToNextLine = true;
    }

    private void HideChoices()
    {
        foreach(GameObject choiceButton in choices)
        {
            choiceButton.SetActive(false);
        }
    }



    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if(splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
            }

            string tagkey  = splitTag[0].Trim();
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
                    if(tagvalue == "true") { isObject = false; } else { isObject = true; }
                    portraitFrame.SetActive(isObject);
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

        if(currentChoices.Count > choices.Length)
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


        for(int i = index; i < choices.Length; i++)
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

        if(canContinueToNextLine)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();
        }      
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        dialogueVariables.variables.TryGetValue(variableName, out variableValue);
        if(variableValue != null)
        {
            Debug.LogWarning("Ink Variable was found to be null: " + variableName);
        }
        return variableValue;
    }


}
