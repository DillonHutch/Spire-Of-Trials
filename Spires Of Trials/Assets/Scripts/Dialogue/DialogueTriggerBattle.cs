using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTriggerBattle : MonoBehaviour
{

    [Header("Ink JSON")]
    [SerializeField] private TextAsset[] inkJSON;


    // Start is called before the first frame update
    void Start()
    {

        if(EncounterManager.ENEMY_TYPE == ENEMY.Knight)
        {
            BattleDialogueManager.GetInstance().EnterDialogueMode(inkJSON[1]);
        }
        else
        {
            BattleDialogueManager.GetInstance().EnterDialogueMode(inkJSON[0]);
        }

        
    }


}
