using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authored by Cayman
/// </summary>
public class InventoryItem
{
    public event Action<InventoryItem> OnDestroyed;
    public ItemType Type { get; private set; }

    public InventoryItem(ItemType type)
    {
        Type = type;
    }

    public void Use() 
    {
        EventManager.Instance.TriggerEvent("itemUseEvent", Type);

        if (Type.DestroyOnUse)
            OnDestroyed.Invoke(this); 
    }
}
