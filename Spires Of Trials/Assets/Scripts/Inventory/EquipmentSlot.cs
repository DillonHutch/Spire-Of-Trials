using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{

    [Header("Item Data")]   
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public ItemType itemType;


    [Header("Item Slots")]
    [SerializeField] private Image itemImage;
    private InventoryManager inventoryManager;
    private EquipmentSOLibrary equipmentSOLibrary;


    [Header("Equipped Slots")]
    [SerializeField] private EquippedSlot headSlot, chestSlot, armSlot, legSlot;




    public GameObject selectedShader;
    public bool thisItemSelected;



    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        equipmentSOLibrary = GameObject.Find("InventoryCanvas").GetComponent<EquipmentSOLibrary>();
    }


    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType)
    {

        //check if the item slot is already full
        if (isFull)
        {
            // If the item slot is full, return the quantity
            return quantity;
        }

        // Update ITEM TYPE

        this.itemType = itemType;


        // Update NAME

        this.itemName = itemName;



        // Update SPRITE
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;

        // Update DESCRIPTION
        this.itemDescription = itemDescription;

        // Update QUANTITY
        this.quantity = 1;
        isFull = true;  


        return 0; // Return 0 if no leftover items

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Handle right click
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {

        if (isFull)
        {
            if (thisItemSelected)
            {
                EquipGear();


            }
            else
            {
                // Handle left click logic here
                inventoryManager.DeselectAllSlots();
                selectedShader.SetActive(true);
                thisItemSelected = true;
                for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
                {
                    if (equipmentSOLibrary.equipmentSO[i].itemName == this.itemName)
                    {
                        equipmentSOLibrary.equipmentSO[i].PreviewEquipment();
                        
                    }
                }

            }
        }
        else
        {
            GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
        }

        

    }

    private void EquipGear()
    {
        if(itemType == ItemType.Head)
        {
            headSlot.EquipGear(itemSprite, itemName, itemDescription);
        }
        else if (itemType == ItemType.Chest)
        {
            chestSlot.EquipGear(itemSprite, itemName, itemDescription);
        }
        else if (itemType == ItemType.Arms)
        {
            armSlot.EquipGear(itemSprite, itemName, itemDescription);
        }
        else if (itemType == ItemType.Legs)
        {
            legSlot.EquipGear(itemSprite, itemName, itemDescription);
        }


        EmptySlot();
    }

    private void EmptySlot()
    {
        itemImage.sprite = emptySprite;
        isFull = false;


    }

    public void OnRightClick()
    {
        // Handle right click logic here
        GameObject itemToDrop = new GameObject(itemName);
        Item newItem = itemToDrop.AddComponent<Item>();
        newItem.quantity = 1;
        newItem.itemName = itemName;
        newItem.sprite = itemSprite;
        newItem.itemDescription = itemDescription;

        SpriteRenderer spriteRenderer = itemToDrop.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemSprite;
        spriteRenderer.sortingOrder = 5;
        spriteRenderer.sortingLayerName = "Player";

        itemToDrop.AddComponent<BoxCollider2D>();

        itemToDrop.transform.position = GameObject.FindWithTag("OverworldPlayer").transform.position + new Vector3(1f, 0, 0);


        this.quantity -= 1;
        if (this.quantity <= 0)
        {
            EmptySlot();
        }

    }



}
