using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    public GameObject inventoryMenu;
    public GameObject equipmentMenu;
    public GameObject questLog;
    public GameObject atlas;

 
    public ItemSlot[] itemSlot;
    public EquipmentSlot[] equipmentSlot;
    public EquippedSlot[] equippedSlot;

    public ItemSO[] itemSOs;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("InventoryMenu"))
        {
            Inventory();
        }
        else if (Input.GetButtonDown("EquipmentMenu"))
        {
            EquipmentMenu();
        }
    }

    private void EquipmentMenu()
    {
        if (equipmentMenu.activeSelf)
        {
            Time.timeScale = 1f; // Resume the game
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(false);
        }

        else
        {
            Time.timeScale = 0f; // Pause the game
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(true);

        }
    }

    void Inventory()
    {
        if (inventoryMenu.activeSelf)
        {
            Time.timeScale = 1f; // Resume the game
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(false);
        }

        else 
        {
            Time.timeScale = 0f; // Pause the game
            inventoryMenu.SetActive(true);
            equipmentMenu.SetActive(false);

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



    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType )
    {

        if(itemType == ItemType.Consumable || itemType == ItemType.QuestItem)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (!itemSlot[i].isFull && itemSlot[i].itemName == itemName || itemSlot[i].quantity == 0)
                {
                    int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);
                    if (leftOverItems > 0)
                        leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription, itemType);


                    return leftOverItems;

                }
            }

            return quantity; // Return the quantity if no slot was available
        }
        else
        {
            for (int i = 0; i < equipmentSlot.Length; i++)
            {
                if (!equipmentSlot[i].isFull && equipmentSlot[i].itemName == itemName || equipmentSlot[i].quantity == 0)
                {
                    int leftOverItems = equipmentSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);
                    if (leftOverItems > 0)
                        leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription, itemType);


                    return leftOverItems;

                }
            }

            return quantity; // Return the quantity if no slot was available
        }

        


    }


    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            equipmentSlot[i].selectedShader.SetActive(false);
            equipmentSlot[i].thisItemSelected = false;
        }


        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].selectedShader.SetActive(false);
            equipmentSlot[i].thisItemSelected = false;
        }

        for (int i = 0; i < equippedSlot.Length; i++)
        {
            equippedSlot[i].selectedShader.SetActive(false);
            equippedSlot[i].thisItemSelected = false;
        }

    }





}



public enum ItemType
{
    Consumable,
    Equipment,
    QuestItem,
    Head,
    Chest,
    Arms,
    Legs,
    None
};