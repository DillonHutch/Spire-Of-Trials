using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialougeTrigger : MonoBehaviour
{

    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;


    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;


    private bool playerInRange;

    

    private void Awake()
    {
        visualCue.SetActive(false);
        playerInRange = false;
    }

    private void Update()
    {
        //if(playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        //{
        //    if(visualCue.tag == "Object")
        //    {
        //        visualCue.SetActive(false);
        //    }
        //    else
        //    {
        //        visualCue.SetActive(true);
        //    }
                
            
        //    if (Input.GetKeyDown(KeyCode.E))
        //    {
        //        DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
        //    }
        //}
        //else
        //{
        //    visualCue.SetActive(false); 
        //}
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "OverworldPlayer")
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "OverworldPlayer")
        {
            playerInRange = false;
        }
    }

}
