using UnityEngine;

[CreateAssetMenu(fileName = "newItem", menuName = "ScriptableObjects/Item Types/Default")]
public class ItemType : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public bool DestroyOnUse { get; private set; } = true;
    [field: SerializeField] public bool IsCombatItem { get; private set; }

    public InventoryItem CreateInventoryItem() => new InventoryItem(this);
}