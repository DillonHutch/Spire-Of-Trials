using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquippedSlot : MonoBehaviour, IPointerClickHandler
{

    [SerializeField]
    private Image slotImage;


    [SerializeField]
    private TMP_Text slotName;

    [SerializeField]
    private Image playerDisplayImage;


    [SerializeField]
    private ItemType itemType = new ItemType();



    private Sprite itemSprite;
    private string itemName;
    private string itemDescription;


    private InventoryManager inventoryManager;
    private EquipmentSOLibrary equipmentSOLibrary;


    private bool slotInUse;

    [SerializeField]
    public GameObject selectedShader;

    [SerializeField]
    public bool thisItemSelected;

    [SerializeField]
    private Sprite emptySprite;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        equipmentSOLibrary = GameObject.Find("InventoryCanvas").GetComponent<EquipmentSOLibrary>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }

    }



    private void OnLeftClick()
    {
        if (thisItemSelected && slotInUse)
        {
            UnEquipGear();
        }
        else
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
            {
                if (equipmentSOLibrary.equipmentSO[i].itemName == this.itemName)
                {
                    equipmentSOLibrary.equipmentSO[i].PreviewEquipment();
                    break;
                }
            }
        }
    }


    private void OnRightClick()
    {
        UnEquipGear();
    }

 

    public void EquipGear(Sprite itemSprite, string itemName, string itemDescription)
    {

        //if something is already equipted
        if (slotInUse)
        {
            UnEquipGear();
        }

        //update image 
        this.itemSprite = itemSprite;
        slotImage.sprite = itemSprite;
        slotName.enabled = false;



        //update Data
        this.itemName = itemName;
        this.itemDescription = itemDescription;

        //update the display image  
        playerDisplayImage.sprite = itemSprite;

        //update player stats

        for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibrary.equipmentSO[i].itemName == this.itemName)
            {
                equipmentSOLibrary.equipmentSO[i].EquipItem();
                break;
            }
        }


        slotInUse = true;

    }

    public void UnEquipGear()
    {
        inventoryManager.DeselectAllSlots();
        inventoryManager.AddItem(itemName, 1, itemSprite, itemDescription, itemType);

        // remove the stats modifier exactly once
        for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibrary.equipmentSO[i].itemName == this.itemName)
            {
                equipmentSOLibrary.equipmentSO[i].UnEquipItem();
                break;
            }
        }

        // clear visuals
        this.itemSprite = emptySprite;
        slotImage.sprite = emptySprite;
        slotName.enabled = true;
        playerDisplayImage.sprite = emptySprite;

        // hide the preview UI
        GameObject.Find("StatManager")
            .GetComponent<PlayerStats>()
            .TurnOffPreviewStats();

        // reset slot state so next equip works correctly
        slotInUse = false;
        thisItemSelected = false;
        itemName = null;
    }



}
