using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(AudioSource), typeof(Animator))]
public class GhostMovement : MonoBehaviour
{
    public float wanderRadius = 10f;
    public AudioClip walkingSound; // Assign a walking sound clip in the inspector
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
    private Vector3 targetPoint; // Target point for navigation
    private float timeStuckThreshold = 2f; // Time in seconds to consider the ghost as stuck
    private float stuckTimer = 0f; // Timer to track how long the ghost has been stuck
    private Vector3 lastPosition; // Last known position, to calculate movement progress
    private float directionChangeInterval = 20f; // Time in seconds to force a direction change
    private float directionTimer = 0f; // Timer to track time since last direction change

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        PickNewTargetPoint();
        lastPosition = transform.position;
    }

    void Update()
    {
        directionTimer += Time.deltaTime;
        if (directionTimer >= directionChangeInterval)
        {
            PickNewDirection(); // Force a new direction every 20 seconds
            directionTimer = 0f;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            PickNewTargetPoint();
        }

        if (Vector3.Distance(transform.position, lastPosition) < 0.01f) // Check if the ghost hasn't moved much
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= timeStuckThreshold) // Ghost is considered stuck
            {
                PickNewDirection(); // Pick a new forward-facing direction
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f; // Reset the stuck timer if the ghost is moving
        }
        lastPosition = transform.position; // Update the last known position

        // Handle walking sound and animation based on speed
        float speed = agent.velocity.magnitude;
        if (speed > 0.1f && !audioSource.isPlaying)
        {
            audioSource.clip = walkingSound;
            audioSource.Play();
        }
        else if (speed <= 0.1f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        animator.SetBool("IsWalking", speed > 0.1f);
    }

    void PickNewTargetPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius + transform.position;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, 1);
        targetPoint = hit.position;
        agent.SetDestination(targetPoint);
    }

    void PickNewDirection()
    {
        float angle = Random.Range(-45, 45); // Adjust the range as needed
        Vector3 forward = transform.forward;
        Vector3 newDirection = Quaternion.Euler(0, angle, 0) * forward * wanderRadius;
        Vector3 newPosition = transform.position + newDirection;
        NavMesh.SamplePosition(newPosition, out NavMeshHit hit, wanderRadius, 1);
        targetPoint = hit.position;
        agent.SetDestination(targetPoint);
    }
}