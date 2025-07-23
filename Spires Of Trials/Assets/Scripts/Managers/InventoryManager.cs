using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    public GameObject inventoryMenu;
    private bool menuActivated = false;
    public ItemSlot[] itemSlot;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O) && menuActivated)
        {
            Time.timeScale = 1f; // Resume the game
            inventoryMenu.SetActive(false);
            menuActivated = false;
        }

        else if (Input.GetKeyDown(KeyCode.O) && !menuActivated)
        {
            Time.timeScale = 0f; // Pause the game
            inventoryMenu.SetActive(true);
            menuActivated = true;
        }
    }



    public void AddItem(string itemName, int quantity, Sprite itemSprite )
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (!itemSlot[i].isFull)
            {
                itemSlot[i].AddItem(itemName, quantity, itemSprite);
                return;
            }
        }


    }
}
