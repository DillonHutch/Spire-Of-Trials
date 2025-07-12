using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Tooltip("Drag in your DoorController here")]
    [SerializeField] private DoorController door;

    [Tooltip("Only react to this tag (leave blank for any)")]
     private string playerTag = "OverworldPlayer";

    // once we’ve tripped, we never clear it
    private bool hasTriggered;


    private void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (hasTriggered) return;

        if (string.IsNullOrEmpty(playerTag) || collision.CompareTag(playerTag))
        {



            hasTriggered = true;
            door.SetPlayerInZone(true);

            // prevent any further trigger or exit events
            enabled = false;
            // (optional) also disable the collider itself:
            // GetComponent<Collider>().enabled = false;
        }
    }
}
