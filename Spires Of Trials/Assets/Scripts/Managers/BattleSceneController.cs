// BattleSceneController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneController : MonoBehaviour
{
    public string battleSceneName = "Battle";
    public List<GameObject> objectsToDisable = new List<GameObject>();
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private PlayerMovement playerMovement;
    private string _previousSceneName;
    private bool isInBattle;

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
        isInBattle = true;
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
    }

    public void EndBattle()
    {
        isInBattle = false;
        StartCoroutine(EndBattleSequence());
    }

    private IEnumerator EndBattleSequence()
    {




        yield return StartCoroutine(screenFader.FadeOut());
        yield return SceneManager.UnloadSceneAsync(battleSceneName);

        // re-enable
        foreach (var go in objectsToDisable)
            if (go != null)
                go.SetActive(true);

        Scene original = SceneManager.GetSceneByName(_previousSceneName);
        if (original.IsValid())
        {
            SceneManager.SetActiveScene(original);
        }

        yield return StartCoroutine(screenFader.FadeIn());
        playerMovement.canMove = true;

        int damage = Mathf.RoundToInt(TimingController.Instance.CombatTimer);
        EventManager.Instance.TriggerEvent("takeDamageEvent", damage);
    }
}
