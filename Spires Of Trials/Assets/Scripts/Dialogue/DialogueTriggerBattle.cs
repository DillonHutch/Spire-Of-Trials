using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTriggerBattle : MonoBehaviour
{

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;


    // Start is called before the first frame update
    void Start()
    {
        BattleDialogueManager.GetInstance().EnterDialogueMode(inkJSON);
    }


}
