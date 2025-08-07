using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OverwordPlayer : MonoBehaviour
{


    [SerializeField] BattleSceneController battleController;

    // cache the layer mask for performance
    int _enemyLayer;

    private void Awake()
    {
        _enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    // OverwordPlayer.cs
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == _enemyLayer)
        {
            
            
            BattleContext.PendingEnemySlotCount = Random.Range(1, 4);

            // start battle
            BattleSceneController battleControllerInstance
                = FindObjectOfType<BattleSceneController>();
            if (battleControllerInstance != null)
            {
                battleControllerInstance.StartBattle();
            }
            else
            {
                Debug.LogError("No BattleSceneController found!");
            }

            this.battleController.objectsToDisable.Remove(other.gameObject);
            Destroy(other.gameObject);
        }
    }


}
