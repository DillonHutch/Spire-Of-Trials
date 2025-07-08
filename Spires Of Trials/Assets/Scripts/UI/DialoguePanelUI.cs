using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

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

    [Header("Audio")]
    [SerializeField] private DialogueAudioInfoSO defaultAudioInfo;
    [SerializeField] private DialogueAudioInfoSO[] audioInfos;
    [SerializeField] private bool makePredictable = false;

    private Dictionary<string, DialogueAudioInfoSO> audioInfosDictionary;
    private DialogueAudioInfoSO currentAudioInfo;

    [SerializeField] private SineWaveText sineWaveText;



    private void Awake()
    {
        contentParent.SetActive(false);
        ResetPanel();

        // audio setup
        currentAudioInfo = defaultAudioInfo;
        audioInfosDictionary = new Dictionary<string, DialogueAudioInfoSO>();
        audioInfosDictionary.Add(defaultAudioInfo.id, defaultAudioInfo);
        foreach (DialogueAudioInfoSO info in audioInfos)
            audioInfosDictionary.Add(info.id, info);
    }

    private void OnEnable()
    {
        EventManager.Instance.StartListening("dialogueStarted", DialogueStarted);
        EventManager.Instance.StartListening("dialogueFinished", DialogueFinished);
        EventManager.Instance.StartListening<(string dialogueLine, List<Choice> dialogueChoices)>("displayDialogue",  data 
            => DisplayDialogue(data.dialogueLine, data.dialogueChoices));

        EventManager.Instance.StartListening<string>("setDialogueAudio", SetCurrentAudioInfo);


    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening("dialogueStarted", DialogueStarted);
        EventManager.Instance.StopListening("dialogueFinished", DialogueFinished);
        EventManager.Instance.StopListening<(string dialogueLine, List<Choice> dialogueChoices)>("displayDialogue", data
            => DisplayDialogue(data.dialogueLine, data.dialogueChoices));

        EventManager.Instance.StopListening<string>("setDialogueAudio", SetCurrentAudioInfo);
    }


    private void SetCurrentAudioInfo(string id)
    {
        if (audioInfosDictionary.TryGetValue(id, out var info))
            currentAudioInfo = info;
        else
            Debug.LogWarning($"No audioInfo with id {id}");
    }

    private void PlayDialogueSound(int charIndex, char c)
    {
        var clips = currentAudioInfo.dialogueTypingSoundClips;
        if (charIndex % currentAudioInfo.frequencyLevel != 0) return;

        // pick a clip
        int idx = makePredictable
          ? Mathf.Abs(c.GetHashCode()) % clips.Length
          : Random.Range(0, clips.Length);

        var inst = RuntimeManager.CreateInstance(clips[idx]);
        if (currentAudioInfo.stopAudioSource)
            inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        if (!makePredictable)
            inst.setPitch(Random.Range(currentAudioInfo.minPitch, currentAudioInfo.maxPitch));
        else
        {
            // predictable pitch version
            int hash = Mathf.Abs(c.GetHashCode());
            float pitch = (hash %
               ((int)(currentAudioInfo.maxPitch * 100) - (int)(currentAudioInfo.minPitch * 100))
            ) / 100f + currentAudioInfo.minPitch;
            inst.setPitch(pitch);
        }

        inst.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        inst.start();
        inst.release();
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

        // Apply wave effect if possible
        if (sineWaveText != null)
        {
            sineWaveText.PrepareWaveText(dialogueLine);
            sineWaveText.enabled = true;
        }
        else
        {
            dialogueText.text = System.Text.RegularExpressions.Regex.Replace(dialogueLine, @"\[(\/?)wave\]", "");
        }

        typingCoroutine = StartCoroutine(TypeDialogue(dialogueText.text, dialogueChoices));
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
            if (Input.GetKeyDown(KeyCode.E))
            {
                dialogueText.maxVisibleCharacters = line.Length;
                EventManager.Instance.TriggerEvent("dialogueLineFinishedTyping");
                break;
            }

            if (c == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                if (c == '>')
                    isAddingRichTextTag = false;
            }
            else
            {
                PlayDialogueSound(totalVisible, line[totalVisible]);
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
