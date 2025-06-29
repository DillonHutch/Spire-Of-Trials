using System;
using UnityEngine;

[CreateAssetMenu(fileName = "newItem", menuName = "ScriptableObjects/Item Types/Healing")]
public class ItemType_Healing : ItemType
{
    [field: SerializeField] public int HealthModifier { get; private set; }
}