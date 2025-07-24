using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    public GameObject inventoryMenu;
    private bool menuActivated = false;
    public ItemSlot[] itemSlot;

    public ItemSO[] itemSOs;


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


    public bool UseItem(string itemName)
    {
        for (int i = 0; i < itemSOs.Length; i++)
        {
          
            if (itemSOs[i].itemName == itemName)
            {
                bool usuable = itemSOs[i].UseItem();
                return usuable; // Exit after using the first matching item
            }
            

        }
        return false; // If no item was found, return false

    }



    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription )
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (!itemSlot[i].isFull && itemSlot[i].itemName == itemName || itemSlot[i].quantity == 0)
            {
                int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
                if(leftOverItems > 0)               
                    leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription);

                               
                return leftOverItems;

            }
        }

        return quantity; // Return the quantity if no slot was available


    }


    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
    }
}
