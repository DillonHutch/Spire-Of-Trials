using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class ItemSO : ScriptableObject
{

    public string itemName;
    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;


    public bool UseItem()
    {
        if(statToChange == StatToChange.Health)
        {
            if(HealthManager.Instance.CurrentLives == HealthManager.Instance.MaxLives)
            {
                return false;
            }
            else
            {
                EventManager.Instance.TriggerEvent("healDamageEvent", amountToChangeStat);
                return true;
            }

                
            //Debug.Log("Used item: " + itemName + " to heal " + amountToChangeStat + " health.");
        }
        else
        {
            Debug.LogWarning("Item " + itemName + " does not change any stats.");
        }

        return false;
    }




    public enum StatToChange
    {
        Health,
        none

    };


}
