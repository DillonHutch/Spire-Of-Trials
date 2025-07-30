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
    public ConsumableSlot[] consumableSlots;

    public ItemSO[] itemSOs;

    public int quantity;


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



    public int AddItem(
     string itemName,
     int quantity,
     Sprite itemSprite,
     string itemDescription,
     ItemType itemType
 )
    {
        if (itemType == ItemType.Consumable)
        {
            int leftover = quantity;
            int pairCount = Mathf.Min(itemSlot.Length, consumableSlots.Length);

            // 1) First try to stack into existing stacks in both menus
            for (int i = 0; i < pairCount && leftover > 0; i++)
            {
                bool canStackGeneral =
                    itemSlot[i].itemName == itemName && !itemSlot[i].isFull;
                bool canStackConsumable =
                    consumableSlots[i].itemName == itemName && !consumableSlots[i].isFull;

                if (canStackGeneral && canStackConsumable)
                {
                    // add to both, then take the worst‐case leftover
                    int leftGen = itemSlot[i].AddItem(
                        itemName, leftover, itemSprite, itemDescription, itemType
                    );
                    int leftCons = consumableSlots[i].AddItem(
                        itemName, leftover, itemSprite, itemDescription, itemType
                    );
                    leftover = Mathf.Max(leftGen, leftCons);
                }
            }

            // 2) Next fill into empty slots in both menus
            for (int i = 0; i < pairCount && leftover > 0; i++)
            {
                bool emptyGen = itemSlot[i].quantity == 0;
                bool emptyCons = consumableSlots[i].quantity == 0;

                if (emptyGen && emptyCons)
                {
                    int leftGen = itemSlot[i].AddItem(
                        itemName, leftover, itemSprite, itemDescription, itemType
                    );
                    int leftCons = consumableSlots[i].AddItem(
                        itemName, leftover, itemSprite, itemDescription, itemType
                    );
                    leftover = Mathf.Max(leftGen, leftCons);
                }
            }

            // 3) If you still have leftovers, you could recurse or just return them:
            return leftover;
        }
        else
        {
            // equipment logic stays the same...
            for (int i = 0; i < equipmentSlot.Length; i++)
            {
                if ((!equipmentSlot[i].isFull && equipmentSlot[i].itemName == itemName)
                    || equipmentSlot[i].quantity == 0)
                {
                    int leftOver = equipmentSlot[i].AddItem(
                        itemName, quantity, itemSprite, itemDescription, itemType
                    );
                    if (leftOver > 0)
                        return AddItem(itemName, leftOver, itemSprite, itemDescription, itemType);
                    return 0;
                }
            }
            return quantity;
        }
    }







    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
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

        for (int i = 0; i < consumableSlots.Length; i++)
        {
            consumableSlots[i].selectedShader.SetActive(false);
            consumableSlots[i].thisItemSelected = false;
        }

    }


    public void RemoveFromOtherSlots(string itemName, ConsumableSlot originSlot)
    {
        // remove one from the first matching ItemSlot
        foreach (ItemSlot slot in itemSlot)
        {
            if (slot.itemName == itemName)
            {
                slot.RemoveOne();
                break;
            }
        }

        // remove one from the other ConsumableSlot (if any)
        foreach (ConsumableSlot slot in consumableSlots)
        {
            if (slot != originSlot && slot.itemName == itemName)
            {
                slot.RemoveOne();
                break;
            }
        }
    }


    // Called when you used from an ItemSlot
    public void RemoveFromOtherSlots(string itemName, ItemSlot originSlot)
    {
        // remove one from the first matching ConsumableSlot
        foreach (ConsumableSlot slot in consumableSlots)
        {
            if (slot.itemName == itemName)
            {
                slot.RemoveOne();
                break;
            }
        }

        // remove one from the other ItemSlot (if any)
        foreach (ItemSlot slot in itemSlot)
        {
            if (slot != originSlot && slot.itemName == itemName)
            {
                slot.RemoveOne();
                break;
            }
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