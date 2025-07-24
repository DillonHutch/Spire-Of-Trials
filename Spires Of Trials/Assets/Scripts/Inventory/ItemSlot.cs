using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ItemSlot : MonoBehaviour, IPointerClickHandler
{

    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;



    [SerializeField] private TMP_Text quantityText;

    [SerializeField] private Image itemImage;

    private InventoryManager inventoryManager;


    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;





    [SerializeField] private int maxNumberOfItems;
    public GameObject selectedShader;
    public bool thisItemSelected;

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {

        //check if the item slot is already full
        if (isFull)
        {
            // If the item slot is full, return the quantity
            return quantity;
        }

        // Update NAME

        this.itemName = itemName;



        // Update SPRITE
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;

        // Update DESCRIPTION
        this.itemDescription = itemDescription;

        // Update QUANTITY
        this.quantity += quantity;
        if(this.quantity >= maxNumberOfItems)
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
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            // Handle right click
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {

        if(thisItemSelected)
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
            }
     
        }
        else
        {
            // Handle left click logic here
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            itemDescriptionNameText.text = itemName;
            itemDescriptionText.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;
            if (itemDescriptionImage.sprite == null)
            {
                itemDescriptionImage.sprite = emptySprite;
            }
        }




    }

    private void EmptySlot()
    {
        quantityText.enabled = false;
        itemImage.sprite = emptySprite;

        itemDescriptionNameText.text = "";
        itemDescriptionText.text = "";
        itemDescriptionImage.sprite = emptySprite;

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
        quantityText.text = this.quantity.ToString();
        if (this.quantity <= 0)
        {
            EmptySlot();
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
