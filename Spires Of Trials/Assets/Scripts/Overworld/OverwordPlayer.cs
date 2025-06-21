using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwordPlayer : MonoBehaviour
{

    [SerializeField] BattleSceneController battleController;

    // Start is called before the first frame update
    void Start()
    {
        battleController.objectsToDisable.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
