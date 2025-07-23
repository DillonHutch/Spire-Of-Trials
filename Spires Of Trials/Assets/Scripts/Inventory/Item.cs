using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // Start is called before the first frame update


    [SerializeField] private string itemName;

    [SerializeField] private int quantity;

    [SerializeField] private Sprite sprite;

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
            inventoryManager.AddItem(itemName, quantity, sprite);
            // Optionally, destroy the item after picking it up
            Destroy(gameObject);
        }
    }

}
