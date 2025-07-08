using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BadNPCScript : MonoBehaviour
{

    [SerializeField] BattleSceneController battleController;

    // Start is called before the first frame update
    void Start()
    {
        battleController.objectsToDisable.Add(gameObject);
    }



}
