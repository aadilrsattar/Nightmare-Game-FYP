using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Required for working with NavMeshAgent
using Mirror;
using System.Collections.Generic;

public class GhostSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    private List<GameObject> activeGhosts = new List<GameObject>();
    private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private int maxGhosts = 2; // Adjusted based on your setup for 1 ghost
    private List<RoomTrigger> roomTriggers = new List<RoomTrigger>();
    public static GhostSpawner Instance { get; private set; }

    private void Awake()
    {

        GameObject[] points = GameObject.FindGameObjectsWithTag("NPCSpawnPoint");
        foreach (GameObject point in points)
        {
            spawnPoints.Add(point.transform);
        }
    }

    private void Start()
    {
        for (int i = 0; i < maxGhosts; i++)
        {
            SpawnGhost();

        }

    }

    [Server]
    private void SpawnGhost()
    {
        Debug.Log("Attempting to spawn ghost");

        if (activeGhosts.Count >= maxGhosts || spawnPoints.Count == 0) return;

        int spawnPointIndex = Random.Range(0, spawnPoints.Count);
        GameObject ghost = Instantiate(ghostPrefab, spawnPoints[spawnPointIndex].position, Quaternion.identity);
        NetworkServer.Spawn(ghost);
        activeGhosts.Add(ghost);
    }

    [Server]
    public void OnGhostCaught()
    {
        if (activeGhosts.Count > 0)
        {
            GameObject toRemove = activeGhosts[0];
            activeGhosts.RemoveAt(0);

            YeetAndRemoveGhost(toRemove);
            ForceResetTrigger();

        }
        if (activeGhosts.Count < maxGhosts)
        {
            SpawnGhost();
        }
    }



    private void YeetAndRemoveGhost(GameObject ghost)
    {
        // Remove specific scripts or components in the desired order
        GhostMovement movementScript = ghost.GetComponent<GhostMovement>();
        if (movementScript != null) { Destroy(movementScript); }

        // Remove the NavMeshAgent
        NavMeshAgent navAgent = ghost.GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            Destroy(navAgent);
        }

        // Remove the BoxCollider
        BoxCollider boxCollider = ghost.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Destroy(boxCollider);
        }

        // Start a coroutine to move the ghost out of the trigger zone
        StartCoroutine(MoveGhostOutOfTriggerZone(ghost));

        // Start a coroutine to remove the ghost after a delay, giving it time to exit the trigger zone
        StartCoroutine(RemoveGhostAfterDelay(ghost, 5f)); // Adjust the delay as needed
    }

    private IEnumerator MoveGhostOutOfTriggerZone(GameObject ghost)
    {
        float moveDuration = 5f; // Duration in seconds over which the ghost will move out of the trigger zone
        float startTime = Time.time;

        // Calculate the target position outside of the trigger zone
        Vector3 targetPosition = ghost.transform.position + new Vector3(0, 10, 0);

        while (Time.time < startTime + moveDuration)
        {
            // Linearly interpolate the position over time
            ghost.transform.position = Vector3.Lerp(ghost.transform.position, targetPosition, (Time.time - startTime) / moveDuration);
            yield return null; // Wait for the next frame
        }
    }

    private IEnumerator RemoveGhostAfterDelay(GameObject ghost, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ghost != null)
        {
            Destroy(ghost);
        }
    }
    public void RegisterRoomTrigger(RoomTrigger trigger)
    {
        if (!roomTriggers.Contains(trigger))
        {
            roomTriggers.Add(trigger);
        }
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
