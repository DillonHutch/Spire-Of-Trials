// BattleSceneController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class BattleSceneController : MonoBehaviour
{
    public string battleSceneName = "Battle";
    public List<GameObject> objectsToDisable = new List<GameObject>();
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private PlayerMovement playerMovement;
    private string _previousSceneName;

    public bool SuppressResume { get; set; } = false;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // auto-assign the fader & player if you forgot in Inspector
        if (screenFader == null)
            screenFader = FindObjectOfType<ScreenFader>();
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        if (playerMovement == null)
            Debug.LogError("BattleSceneController: no PlayerMovement found in scene!");
        if (screenFader == null)
            Debug.LogError("BattleSceneController: no ScreenFader found in scene!");


    }


    public void StartBattle()
    {
        
        _previousSceneName = SceneManager.GetActiveScene().name;
        playerMovement.canMove = false;
        
        StartCoroutine(LoadBattleScene());
    }

    private IEnumerator LoadBattleScene()
    {

        
        yield return StartCoroutine(screenFader.FadeOut());

       

        // only loop if we actually have items
        foreach (var go in objectsToDisable)
            if (go != null)
                go.SetActive(false);

        var loadOp = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        yield return loadOp;
        Scene battleScene = SceneManager.GetSceneByName(battleSceneName);
        if (battleScene.IsValid())
        {
            SceneManager.SetActiveScene(battleScene);
            TimingController.Instance.ResetCombatTimer();
        }
        yield return StartCoroutine(screenFader.FadeIn());

        EventManager.Instance.TriggerEvent("battleSceneLoaded");
    }

    public void EndBattle()
    {
        
        StartCoroutine(EndBattleSequence());
    }

    private IEnumerator EndBattleSequence()
    {
        yield return StartCoroutine(screenFader.FadeOut());
        yield return SceneManager.UnloadSceneAsync(battleSceneName);
        foreach (var go in objectsToDisable)
            if (go != null) go.SetActive(true);
        var original = SceneManager.GetSceneByName(_previousSceneName);
        if (original.IsValid())
            SceneManager.SetActiveScene(original);
        yield return StartCoroutine(screenFader.FadeIn());

        // announce that combat is over
        EventManager.Instance.TriggerEvent("battleSceneUnLoaded");

        // if this fight was driven by dialogue, defer resuming until after dialogue
        if (SuppressResume)
        {
            EventManager.Instance.StartListening("dialogueFinished", ResumeAfterDialogue);
        }
        else
        {
            ResumeControl();
        }

        // reset the flag for next time
        SuppressResume = false;
    }

    private void ResumeAfterDialogue()
    {
        // only run once, then drop the listener
        EventManager.Instance.StopListening("dialogueFinished", ResumeAfterDialogue);
        ResumeControl();
    }

    private void ResumeControl()
    {
        EventManager.Instance.TriggerEvent("StartPlayerMovement");
        EventManager.Instance.ChangeInputEventContext(InputEventContext.DEFAULT);
    }
}
