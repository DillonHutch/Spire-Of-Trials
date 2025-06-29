using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored by Cayman
/// </summary>
public class PlayerInventoryDisplay : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private GameObject noItemsObject;

    [Header("Selected Info Panel")]
    [SerializeField] private GameObject selectedInfoObject;
    [SerializeField] private TextMeshProUGUI selectedTitle, selectedDescription;

    [Header("Display Settings")]
    [SerializeField] private GameObject displayItemPrefab;
    [SerializeField] private Transform displayParent;
    [SerializeField] private int displaySpacing = 230;

    [Header("Prompts")]
    [SerializeField] private GameObject invalidUsePromptObject;
    [SerializeField] private GameObject usePromptObject;

    private Inventory playerInventory;

    private int selectedIndex;
    private List<GameObject> displays = new();

    private float displayOrigin;
    private bool isEnabled = false;

    private void Start()
    {
        print(GameObject.FindGameObjectWithTag("OverworldPlayer"));
        //Not sure how scene transitions and such will be handled later, so this is temporary
        playerInventory = GameObject.FindGameObjectWithTag("OverworldPlayer").GetComponent<Inventory>();

        playerInventory.OnInventoryUpdate += UpdateDisplay;

        displayOrigin = displayParent.transform.localPosition.x;

        UpdateDisplay();
    }

    private void Update()
    {
        //Enable/Disable the inventory
        if (Input.GetKeyDown(KeyCode.R))
        {
            isEnabled = !isEnabled;

            //Freezing movement like this seems sketch, so this is temporary
            FindObjectOfType<PlayerMovement>().canMove = !isEnabled;

            UpdateDisplay();
        }

        if (!isEnabled) 
            return;

        //Update the selected item
        if (Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex--;
            if (selectedIndex < 0)
                selectedIndex = 0;

            UpdateDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex++;
            if (selectedIndex >= displays.Count)
                selectedIndex = displays.Count - 1;

            UpdateDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            //This is where we check whether we're in combat, but its not implemented yet so I have != false
            if (playerInventory.GetNonEmptyItemAt(selectedIndex).Type.IsCombatItem != false)
            {
                invalidUsePromptObject.SetActive(true);
            }
            else
                usePromptObject.SetActive(true);
        }
    }

    /// <summary>
    /// Uses the selected item
    /// </summary>
    public void UseItem()
    {
        playerInventory.GetNonEmptyItemAt(selectedIndex).Use();
    }

    private void UpdateDisplay()
    {
        inventoryObject.SetActive(isEnabled);

        //Adjusts the currently spawned display count
        if (displays.Count != playerInventory.Size)
        {
            //If theres too many displays, remove until matching
            while (displays.Count > playerInventory.Size)
            {
                Destroy(displays[0]);
                displays.RemoveAt(0);
            }

            //If theres too few displays, add until matching
            while (displays.Count < playerInventory.Size)
            {
                displays.Add(Instantiate(displayItemPrefab, displayParent, false));
            }
        }

        //If the player was selecting an index past whats now available (i.e. the last item and it was removed), set the index to the last item
        if (selectedIndex >= displays.Count)
            selectedIndex = displays.Count - 1;

        //If the inventory is empty, don't even display the item info panel
        selectedInfoObject.SetActive(displays.Count != 0);

        //If the inventory is empty, show the "NO ITEMS" text
        noItemsObject.SetActive(displays.Count == 0);
        
        //Adjust all of the displays, player inventory ignores empty slots
        for (int i = 0; i < displays.Count; i++) 
        {
            InventoryItem item = playerInventory.GetNonEmptyItemAt(i);
            foreach (Image image in displays[i].GetComponentsInChildren<Image>())
                image.sprite = item.Type.Sprite;

            if (i == selectedIndex)
            {
                selectedDescription.text = item.Type.Description;
                selectedTitle.text = item.Type.Name;
            }
        }

        //Adjust the display shift so the selected item is displayed in the middle
        displayParent.transform.localPosition = new Vector3((-selectedIndex * displaySpacing) + displayOrigin, 0, 0);
    }
}
