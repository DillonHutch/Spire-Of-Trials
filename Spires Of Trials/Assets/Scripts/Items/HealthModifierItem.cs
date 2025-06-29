using UnityEngine;

public class HealthModifierItem : MonoBehaviour
{
    private void Start()
    {
        EventManager.Instance.StartListening<ItemType>("itemUseEvent", OnItemUse);
    }

    private void OnItemUse(ItemType type)
    {
        if (type is not ItemType_Healing)
            return;

        int healthModifier = (type as ItemType_Healing).HealthModifier;

        if (healthModifier > 0)
            EventManager.Instance.TriggerEvent("healDamageEvent", healthModifier);
        else
            EventManager.Instance.TriggerEvent("takeDamageEvent", healthModifier);
    }
}