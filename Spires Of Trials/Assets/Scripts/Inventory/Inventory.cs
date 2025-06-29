using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authored by Cayman
/// </summary>
public class Inventory : MonoBehaviour
{
    #region Serialized Fields
    /// <summary>
    /// The amount of items that can be held in the inventory
    /// </summary>
    [field: SerializeField] public int Capacity { get; private set; } = 5;

    [SerializeField] private List<ItemType> startingItems;
    #endregion

    #region Public Fields
    //Event tag allows external usage without overwritting with '=', and prohibits external invoking
    public event Action OnInventoryUpdate;
    public event Action<InventoryItem> OnItemAdded;
    public event Action<InventoryItem> OnItemRemoved;
    public event Action OnInventoryCleared;

    /// <summary>
    /// The amount of items currently in the inventory
    /// </summary>
    public int Size
    {
        get
        {
            int size = 0;
            for (int i = 0; i < inventoryItems.Length; i++)
            {
                if (inventoryItems[i] != null)
                    size++;
            }
            return size;
        }
    }
    #endregion

    #region Private Fields
    private InventoryItem[] inventoryItems;
    #endregion

    #region Unity Monobehaviours
    private void Start()
    {
        //Binds all specific events to the InventoryUpdate event too
        OnInventoryCleared += OnInventoryUpdate;
        OnItemAdded += (x) => OnInventoryUpdate.Invoke(); //Lambda to drop the argument
        OnItemRemoved += (x) => OnInventoryUpdate.Invoke();

        OnItemAdded += HookNewItem;
        OnItemRemoved += UnhookItem;

        inventoryItems = new InventoryItem[Capacity];

        for (int i = 0; i < startingItems.Count; i++)
            AddItem(startingItems[i].CreateInventoryItem());
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Attempts to add an item to the inventory
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>Whether the item was successfully added</returns>
    public bool AddItem(InventoryItem item)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = item;
                OnItemAdded.Invoke(item);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes all instances of the given item from the inventory
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Whether any item was removed</returns>
    public bool RemoveItem(InventoryItem item)
    {
        bool removed = false;
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i]?.Equals(item) ?? false)
            {
                inventoryItems[i] = null;
                removed = true; //Flag in order to remove ALL instances of the item
            }
        }

        if (removed)
            OnItemRemoved.Invoke(item);

        return removed;
    }

    /// <summary>
    /// Clears the inventory
    /// </summary>
    public void Clear()
    {
        foreach (InventoryItem item in inventoryItems)
            RemoveItem(item);

        OnInventoryCleared.Invoke();
    }

    /// <summary>
    /// Gets the item at the given inventory index. Returns null for empty slot
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public InventoryItem GetItemAt(int index) => inventoryItems[index];

    /// <summary>
    /// Gets the item at the given inventory index, ignoring null slots.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public InventoryItem GetNonEmptyItemAt(int index)
    {
        int curr = 0;
        foreach (InventoryItem inventoryItem in inventoryItems)
        {
            if (inventoryItem != null)
            {
                if (curr == index)
                    return inventoryItem;

                curr++;
            }
        }
        return null;
    }

    /// <summary>
    /// Sets the item at the given index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    /// <returns>The item previously at the index. Returns null for empty slot</returns>
    public InventoryItem SetItemAt(int index, InventoryItem item)
    {
        InventoryItem originalItem = inventoryItems[index];
        inventoryItems[index] = item;

        if (originalItem != null)
            OnItemRemoved.Invoke(originalItem);

        OnItemAdded.Invoke(item);

        return originalItem;
    }

    /// <summary>
    /// Allows iteration through all existing items in the inventory, and not just each index
    /// </summary>
    /// <returns></returns>
    public IEnumerator<InventoryItem> GetEnumerator()
    {
        foreach (InventoryItem item in inventoryItems)
            yield return item;
    }
    #endregion

    #region Private Methods
    private void HookNewItem(InventoryItem item)
    {
        item.OnDestroyed += ItemDestroyed;
    }

    private void UnhookItem(InventoryItem item)
    {
        item.OnDestroyed -= ItemDestroyed;
    }

    //This is necessary to unhook the item since you can't unhook lambdas
    //AND RemoveItem returns a bool (that doesn't match the event type of the Item's OnDestroyed) 
    private void ItemDestroyed(InventoryItem item)
    {
        RemoveItem(item);
    }
    #endregion
}
