using QuickStart;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    private bool playerInRoom = false;
    private bool ghostInRoom = false;

    // Reference to the ItemSwitch component on the player
    private ItemSwitch playerItemSwitch;

    void Start()
    {
        GhostSpawner spawner = FindObjectOfType<GhostSpawner>();
        if (spawner != null)
        {
            spawner.RegisterRoomTrigger(this);
        }
        else
        {
            Debug.LogWarning("GhostSpawner not found in the scene.");
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRoom = true;
            playerItemSwitch = other.GetComponent<ItemSwitch>(); // Get the ItemSwitch component
            CheckForBothInRoom();
        }
        else if (other.CompareTag("Ghost"))
        {
            ghostInRoom = true;
            CheckForBothInRoom();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRoom = false;
            playerItemSwitch = null; // Clear the reference
        }
        else if (other.CompareTag("Ghost"))
        {
            ghostInRoom = false;
        }
    }

    private void CheckForBothInRoom()
    {
        if (playerInRoom && ghostInRoom && playerItemSwitch != null)
        {
            playerItemSwitch.SwitchToAlternateItem();
            PlayerScript playerScript = FindObjectOfType<PlayerScript>(); // Find the player script in your scene
            if (playerScript != null)
            {
                playerScript.SetIsNearGhost(true);

            }
        }
    }

    public void ResetTriggerState()
    {
        PlayerScript playerScript = FindObjectOfType<PlayerScript>(); // Find the player script in your scene
        if (playerScript != null)
        {
            playerScript.SetIsNearGhost(true);

        }
        playerInRoom = false;
        ghostInRoom = false;
        playerScript.SetIsNearGhost(false);
        // Any other reset logic...
    }

    public void ForceResetTrigger()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
            triggerCollider.enabled = true;
        }
    }

}