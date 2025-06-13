using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneController : MonoBehaviour
{
    [Tooltip("Exact name as in Build Settings")]
    public string battleSceneName = "Battle";

    private string _previousSceneName;

    public List<GameObject> objectsToDisable = new List<GameObject>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call this to begin the battle transition.
    /// </summary>
    public void StartBattle()
    {

        foreach(GameObject objectToDisable in objectsToDisable)
        {
            objectToDisable.SetActive(false);
        }

        // Remember where we were
        _previousSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadBattleScene());
    }

    private IEnumerator LoadBattleScene()
    {
        // Optionally play your fade‐out here...
        var loadOp = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        yield return loadOp; // wait until loaded

        // Make the Battle scene active, if needed:
        var battleScene = SceneManager.GetSceneByName(battleSceneName);
        if (battleScene.IsValid())
            SceneManager.SetActiveScene(battleScene);

        // Optionally play your fade‐in here...
    }

    /// <summary>
    /// Call this when the battle is over.
    /// </summary>
    public void EndBattle()
    {
        foreach (GameObject objectToDisable in objectsToDisable)
        {
            objectToDisable.SetActive(true);
        }

        
        StartCoroutine(UnloadBattleScene());
      
    }

    private IEnumerator UnloadBattleScene()
    {
        // Optionally fade out of battle here...
        var unloadOp = SceneManager.UnloadSceneAsync(battleSceneName);
        yield return unloadOp; // wait until unloaded

     

        // Restore the original scene as active
        var original = SceneManager.GetSceneByName(_previousSceneName);
        if (original.IsValid())
            SceneManager.SetActiveScene(original);
      

    }
}
