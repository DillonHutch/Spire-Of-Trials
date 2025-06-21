using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldBossConcept : MonoBehaviour
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "OverworldPlayer")
        {

            EncounterManager.ENEMY_TYPE = ENEMY.Knight;

            battleController.StartBattle();
            battleController.objectsToDisable.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
