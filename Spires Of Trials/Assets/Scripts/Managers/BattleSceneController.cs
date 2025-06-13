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

    private void Awake() => DontDestroyOnLoad(gameObject);

    public void StartBattle()
    {
   
        _previousSceneName = SceneManager.GetActiveScene().name;
        playerMovement.canMove = false;
        StartCoroutine(LoadBattleScene());
    }

    private IEnumerator LoadBattleScene()
    {
        yield return StartCoroutine(screenFader.FadeOut());
        foreach (var obj in objectsToDisable)
            obj.SetActive(false);
        var loadOp = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        yield return loadOp;
        var battleScene = SceneManager.GetSceneByName(battleSceneName);
        if (battleScene.IsValid())
            SceneManager.SetActiveScene(battleScene);
        yield return StartCoroutine(screenFader.FadeIn());
    }

    public void EndBattle()
    {
        foreach (var obj in objectsToDisable)
            obj.SetActive(true);
        StartCoroutine(UnloadBattleScene());
    }

    private IEnumerator UnloadBattleScene()
    {
        yield return StartCoroutine(screenFader.FadeOut());
        var unloadOp = SceneManager.UnloadSceneAsync(battleSceneName);
        yield return unloadOp;
        var original = SceneManager.GetSceneByName(_previousSceneName);
        if (original.IsValid())
            SceneManager.SetActiveScene(original);
        yield return StartCoroutine(screenFader.FadeIn());
        
        //yield return new WaitForSeconds(.5f);
        playerMovement.canMove = true;
    }
}

