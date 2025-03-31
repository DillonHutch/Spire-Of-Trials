using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelLoader : MonoBehaviour
{


    [SerializeField] Animator transition;
    [SerializeField] float transitionTime;




    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening<string> ("LoadNextLevel", LoadNextLevel);
        }
        else
        {
            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }
    }


    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening<string>("LoadNextLevel", LoadNextLevel);
        }
    }


    void LoadNextLevel(string sceneToLoad)
    {

        StartCoroutine(LoadLevel(sceneToLoad));
    }


    IEnumerator LoadLevel(string sceneToLoad)
    {
        //play animatioon
        transition.SetTrigger("Start");


        //wait

        yield return new WaitForSeconds(transitionTime);

        //load scene

        SceneManager.LoadScene(sceneToLoad);
    }
}
