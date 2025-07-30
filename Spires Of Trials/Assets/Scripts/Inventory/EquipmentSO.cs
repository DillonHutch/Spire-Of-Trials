using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class EquipmentSO : ScriptableObject
{

    public string itemName;
    public float attack, defense, skillCost, ability;


    [SerializeField]
    private Sprite itemSprite;


    public void PreviewEquipment()
    {
        GameObject.Find("StatManager").GetComponent<PlayerStats>().
            previewEquipmentStats(attack, defense, skillCost, ability, itemSprite);
    }

    public void EquipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();   
        playerStats.attack += attack;
        playerStats.defense += defense;
        playerStats.skillCost -= skillCost;
        playerStats.ability += ability;

       

        playerStats.UpdateEquipmentStats();

    }


    public void UnEquipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerStats.attack -= attack;
        playerStats.defense -= defense;
        playerStats.skillCost += skillCost;
        playerStats.ability -= ability;

        playerStats.UpdateEquipmentStats();

    }

}
