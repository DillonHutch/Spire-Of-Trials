using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ConsumableSlot : MonoBehaviour, IPointerClickHandler
{

    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public ItemType itemType;



    [SerializeField] private TMP_Text quantityText;

    [SerializeField] private Image itemImage;

    private InventoryManager inventoryManager;


    [SerializeField] private int maxNumberOfItems;
    public GameObject selectedShader;
    public bool thisItemSelected;

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
        this.quantity += quantity;
        if (this.quantity >= maxNumberOfItems)
        {
            quantityText.text = maxNumberOfItems.ToString();
            quantityText.enabled = true;

            isFull = true;

            //return the leftover quantity
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;

        }


        //update quintity text
        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;

        return 0; // Return 0 if no leftover items

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }

    }

    public void OnLeftClick()
    {

        if (thisItemSelected)
        {
            bool usable = inventoryManager.UseItem(itemName);
            if (usable)
            {
                this.quantity -= 1;
                quantityText.text = this.quantity.ToString();
                if (this.quantity <= 0)
                {
                    EmptySlot();
                }
                inventoryManager.RemoveFromOtherSlots(itemName, this);

                EventManager.Instance.TriggerEvent("openFightMenu");    

            }

        }
        else
        {
            // Handle left click logic here
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;

        }




    }

    // In ConsumableSlot (and similarly in ItemSlot)
    public void RemoveOne()
    {
        quantity--;
        quantityText.text = quantity.ToString();
        if (quantity <= 0)
            EmptySlot();
    }





    private void EmptySlot()
    {
        quantityText.enabled = false;
        itemImage.sprite = emptySprite;

    }


    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

}
