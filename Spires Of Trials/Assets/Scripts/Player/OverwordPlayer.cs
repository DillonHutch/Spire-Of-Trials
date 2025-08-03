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

            //Debug.Log($"OverwordPlayer collided with enemy: {other.name}");
            // stash tag & random slot‑count
            BattleContext.PendingEnemyTag = other.tag;
            BattleContext.PendingEnemySlotCount = Random.Range(1, 4);

            // start battle

            if (battleController != null)
            {
                battleController.StartBattle();
            }
            else
            {
                Debug.LogError("No BattleSceneController found!");
            }

            Debug.Log(other.name + " collided with OverwordPlayer, starting battle...");

            battleController.objectsToDisable.Remove(other.gameObject);
            Destroy(other.gameObject);
        }
    }


}
