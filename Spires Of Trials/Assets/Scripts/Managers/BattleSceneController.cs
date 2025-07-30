// BattleSceneController.cs
using FMODUnity;
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


    private bool suppressResume = false;

    [SerializeField]
    private Canvas itemCanvas;

    [SerializeField]
    private Camera mainCamera;


    private Camera battleCamera;




    private void OnEnable()
    {
        EventManager.Instance.StartListening("StartBattle", StartBattle);
        EventManager.Instance.StartListening("EndBattle", EndBattle);
        EventManager.Instance.StartListening("battleSceneControllerSupress", BattleSupress);
    }


    private void OnDisable()
    {
        EventManager.Instance.StopListening("StartBattle", StartBattle);
        EventManager.Instance.StopListening("EndBattle", EndBattle);
        EventManager.Instance.StopListening("battleSceneControllerSupress", BattleSupress);
    }


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


    private void BattleSupress()
    {
        suppressResume = true;
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


        battleCamera = GameObject.Find("BattleCamera").GetComponent<Camera>();
        itemCanvas.worldCamera = battleCamera;

        //EventManager.Instance.TriggerEvent("battleSceneLoaded");
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

        EventManager.Instance.ChangeInputEventContext(InputEventContext.DEFAULT);

        Debug.Log(suppressResume);

        // if this fight was driven by dialogue, defer resuming until after dialogue
        if (suppressResume)
        {
            EventManager.Instance.StartListening("dialogueFinished", ResumeAfterDialogue);
        }
        else
        {
            ResumeControl();
        }

        // reset the flag for next time
        suppressResume = false;

        AudioManager.instance.SetMusic(MusicEnum.Forest);

        itemCanvas.worldCamera = mainCamera;

        // announce that combat is over
        EventManager.Instance.TriggerEvent("battleSceneUnLoaded");
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
        
    }
}
