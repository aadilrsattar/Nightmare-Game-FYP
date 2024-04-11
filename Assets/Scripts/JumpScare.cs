using System.Collections;
using UnityEngine;

public class JumpScare : MonoBehaviour
{
    public GameObject ZombiePrefab; // Reference to the zombie prefab with the scream sound attached.
    public int delay;               // Delay before the zombie is destroyed and the trigger box is reactivated.

    private Collider triggerCollider;       // Reference to this GameObject's Collider component.
    private int triggerCount = 0;           // Counter for player trigger entries.
    private int triggerThreshold;           // Random entry threshold for activating the scare.

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();  // Get the Collider component attached to this GameObject.
        triggerThreshold = Random.Range(5, 11);      // Set a random trigger threshold between 5 and 11 (exclusive of 11).
    }

    void OnTriggerEnter(Collider other)
    {
        // Ensure that it's the player triggering the event
        if (other.CompareTag("Player"))
        {
            triggerCount++; // Increment the trigger count on player entry.

            // Adjusted logic based on your note, make sure this is the intended behavior.
            if (triggerCount / 2 == triggerThreshold)
            {
                // Spawn the zombie prefab at the position of this TriggerBox.
                GameObject spawnedZombie = Instantiate(ZombiePrefab, transform.position, Quaternion.identity);

                // Deactivate the Collider component to prevent re-triggering during the jump scare.
                triggerCollider.enabled = false;

                // Start the coroutine to handle post-jump scare actions.
                StartCoroutine(EndJump(spawnedZombie));
            }
        }
    }

    IEnumerator EndJump(GameObject spawnedZombie)
    {
        yield return new WaitForSeconds(delay);
        Destroy(spawnedZombie); // Destroy the spawned zombie after the delay.

        // Reactivate the Collider for future triggers.
        triggerCollider.enabled = true;

        // Reset and choose a new trigger threshold for unpredictability.
        triggerCount = 0;
        triggerThreshold = Random.Range(5, 11);
    }
}
