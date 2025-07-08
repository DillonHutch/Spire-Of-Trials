using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BadNPCScript : MonoBehaviour
{

    [SerializeField] BattleSceneController battleController;

    [SerializeField] private GameObject battlePrefab; // assign in inspector



    // Start is called before the first frame update
    void Start()
    {
        battleController.objectsToDisable.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if(collision.gameObject.tag == "OverworldPlayer")
    //    {

    //        EncounterManager.NextBattleEnemyPrefab = battlePrefab;

    //        battleController.StartBattle();
    //    }

    //    battleController.objectsToDisable.Remove(gameObject);
        
    //    Destroy(gameObject);
    //}

}
