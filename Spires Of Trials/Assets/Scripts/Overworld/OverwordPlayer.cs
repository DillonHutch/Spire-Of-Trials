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

             
            // stash the enemy’s tag
            BattleContext.PendingEnemyTag = other.tag;
            other.gameObject.SetActive(false);

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
