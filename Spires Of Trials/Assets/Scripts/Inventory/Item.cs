using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // Start is called before the first frame update


    public string itemName;

    public int quantity;

    public Sprite sprite;

    [TextArea]
    public string itemDescription;

    private InventoryManager inventoryManager;

    void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OverworldPlayer"))
        {
            // Add the item to the inventory
            int leftOverItems = inventoryManager.AddItem(itemName, quantity, sprite, itemDescription);
            if (leftOverItems <= 0)
            {
                // If there are leftover items, you might want to handle them (e.g., spawn a new item)
                Destroy(gameObject);
            }
            else
            {
                quantity = leftOverItems;
            }

        }
    }

}
