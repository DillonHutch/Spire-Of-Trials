using UnityEngine;

public class Monster : EnemyParent
{


    protected override void Die()
    {
        base.Die();
        Debug.Log("Monster defeated!");
        // Add custom logic for Monster death, like triggering a boss phase
    }
}
