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
        for (int i = 0; i < objectsToDisable.Count; i++)
        {
            objectsToDisable[i].SetActive(false);
        }
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
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

        Scene original = SceneManager.GetSceneByName(_previousSceneName);
        if (original.IsValid())
        {
            SceneManager.SetActiveScene(original);
        }
        for (int i = 0; i < objectsToDisable.Count; i++)
        {
            objectsToDisable[i].SetActive(true);
        }
        yield return StartCoroutine(screenFader.FadeIn());
        playerMovement.canMove = true;

        int damage = Mathf.RoundToInt(TimingController.Instance.CombatTimer);
        EventManager.Instance.TriggerEvent("takeDamageEvent", damage);
    }
}
