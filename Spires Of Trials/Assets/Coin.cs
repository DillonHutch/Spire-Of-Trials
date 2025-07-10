using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private string resourceName = "coins";
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // only trigger when the player picks it up
        if (!other.CompareTag("OverworldPlayer"))
            return;

        // this will:
        // 1) increment ResourceManager.Instance.resourceCounts["coins"] by 1
        // 2) fire events "resourceAdded_coins" and "resourceChanged"
        ResourceManager.Instance.AddResource(resourceName, amount);

        Destroy(gameObject);
    }
}
