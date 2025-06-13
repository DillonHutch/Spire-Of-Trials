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
        StartCoroutine(EndBattleSequence());
    }

    private IEnumerator EndBattleSequence()
    {
        // 1) Fade out to black
        yield return StartCoroutine(screenFader.FadeOut());

        // 2) Unload the battle scene
        yield return SceneManager.UnloadSceneAsync(battleSceneName);

        // 3) Switch back & re-enable your gameplay objects
        var original = SceneManager.GetSceneByName(_previousSceneName);
        if (original.IsValid())
            SceneManager.SetActiveScene(original);

        foreach (var obj in objectsToDisable)
            obj.SetActive(true);

        // 4) Fade back in
        yield return StartCoroutine(screenFader.FadeIn());

        // Finally, allow player movement again
        playerMovement.canMove = true;
    }

}

