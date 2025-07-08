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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == _enemyLayer)
        {

            //Debug.Log($"[Overworld] Trigger hit: {other.name} (layer {other.gameObject.layer}, tag {other.tag})");

            //if (other.gameObject.layer != _enemyLayer)
            //{
            //    Debug.Log("[Overworld] Not on Enemy layer, exiting");
            //    return;
            //}

            //Debug.Log("[Overworld] Enemy layer OK, starting battle...");



            // stash the enemy’s tag
            BattleContext.PendingEnemyTag = other.tag;
            

            // load the battle
            var bc = FindObjectOfType<BattleSceneController>();
            if (bc != null)
                bc.StartBattle();
            else
                Debug.LogError("No BattleSceneController found!");
            battleController.objectsToDisable.Remove(other.gameObject);
            Destroy(other.gameObject);

        }
    }
}
