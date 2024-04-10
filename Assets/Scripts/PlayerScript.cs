using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        public GameObject playerCameraPrefab; // Reference to the player camera prefab
        private GameObject playerCameraInstance; // Instance of the player camera for this player
        public Animator animator; // Reference to the Animator component

        public bool useFirstPersonCamera = true; // Set to true for a first-person view

        public float mouseSensitivity = 2f; // Adjust sensitivity to your liking
        public float rotationSpeedMultiplier = 0.5f; // Adjust rotation speed multiplier to your liking
        public float moveSpeed = 4f; // Adjust movement speed to your liking
        public float sprintSpeedMultiplier = 2f; // Adjust sprint speed multiplier to your liking
        public float jumpForce = 5f; // Adjust jump force to your liking

        private float rotationX = 0f; // Current rotation around the X-axis (up and down)
        private bool isGrounded = true; // Check if the player is grounded

        private bool isNearGhost = false;
        private bool isActionOnCooldown = false;
        public GhostSpawner ghostSpawner; // Assign this in the Inspector

        public override void OnStartLocalPlayer()
        {
            // Instantiate the player camera prefab and parent it to the player object
            playerCameraInstance = Instantiate(playerCameraPrefab, transform.position, Quaternion.identity);
            playerCameraInstance.transform.SetParent(transform);

            // Adjust the position of the player camera as needed
            playerCameraInstance.transform.localPosition = new Vector3(0, 1.66f, 0); // Third-person camera position

            // Get the Animator component attached to the player
            animator = GetComponent<Animator>();

            // Freeze rotation to prevent physical collisions
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.freezeRotation = true;
            }

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            if (isLocalPlayer) // Ensures this setup is only done for the local player
            {
                ghostSpawner = FindObjectOfType<GhostSpawner>();
                if (ghostSpawner == null)
                {
                    Debug.LogError("Failed to find GhostSpawner on the network.");
                }

            }

            CmdRequestScores();

        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Assuming each player has a unique NetID and ScoresManager manages scores based on this ID
            var scoresManager = FindObjectOfType<ScoresManager>();
            if (scoresManager != null)
            {
                // Initialize the player's score. You can define this method in ScoresManager.
                scoresManager.InitializePlayerScore(connectionToClient, 0);
            }
        }
        void Update()
        {
            if (!isLocalPlayer) return;

            // Mouse input for rotation
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate the player horizontally based on mouse input
            transform.Rotate(Vector3.up, mouseX);

            // Calculate vertical rotation incrementally
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Limit vertical rotation to prevent flipping

            // Rotate the player camera vertically
            playerCameraInstance.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

            // Determine the current movement speed
            float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintSpeedMultiplier : moveSpeed;

            // Movement using WASD and arrow keys
            float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * currentMoveSpeed;
            float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * currentMoveSpeed;

            // Move the player
            transform.Translate(new Vector3(moveX, 0f, moveZ));

            // Check if the player is grounded
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);

            // Jumping
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            // Update the IsWalking parameter in the Animator
            animator.SetBool("IsWalking", Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f);

            CheckForScoringOpportunity();

        }
        private void CheckForScoringOpportunity()
        {
            if (Input.GetKeyDown(KeyCode.G) && isNearGhost && !isActionOnCooldown)
            {
                CmdCatchGhost(); // Send a command to the server to catch the ghost
                isActionOnCooldown = true; // Start cooldown on the client side immediately
                                           // Note: Actual cooldown logic will be handled in the RpcHandleGhostCaught method
            }
        }

        IEnumerator WaitAndHandleGhostCaught()
        {
            yield return new WaitForSeconds(15); // Wait for 15 seconds

            ghostSpawner.OnGhostCaught(); // Handle the ghost being caught after the delay

            isActionOnCooldown = false; // Allow the action to be triggered again
            isNearGhost = false;
        }


        // This method is assumed to be called based on proximity detection logic, similar to previous examples
        public void SetIsNearGhost(bool isNear)
        {
            isNearGhost = isNear;
        }

        [Command]
        private void CmdCatchGhost()
        {
            // Assuming the ghost catching logic might involve score increase and
            // potentially other server-side effects
            if (isNearGhost) // Ensure isNearGhost or equivalent logic is validated server-side
            {
                ScoresManager scoresManager = FindObjectOfType<ScoresManager>();
                if (scoresManager != null)
                {
                    scoresManager.AddScore(connectionToClient, 1);
                }
                ghostSpawner.OnGhostCaught(); // Notify the GhostSpawner that a ghost has been caught on the server

                // If the ghost removal or disabling happens here, ensure it's properly networked
                // For example, ghostSpawner could disable the ghost and respawn it as needed

                RpcHandleGhostCaught(); // Call a ClientRpc to handle client-side effects, if necessary
            }
        }

        [ClientRpc]
        private void RpcHandleGhostCaught()
        {
            // Handle any client-specific effects here, such as playing animations, sounds,
            // or showing messages to the player that they caught a ghost

            StartCoroutine(WaitAndReset()); // Start cooldown and potentially reset local player state
        }

        IEnumerator WaitAndReset()
        {
            yield return new WaitForSeconds(15); // Wait for 15 seconds

            isActionOnCooldown = false; // Allow the action to be triggered again
            isNearGhost = false; // Reset proximity flag
        }
        [Command]
        private void CmdRequestScores()
        {
            ScoresManager scoresManager = FindObjectOfType<ScoresManager>();
            if (scoresManager != null)
            {
                scoresManager.AddScore(connectionToClient, 0);
            }
            scoresManager.SendCurrentScoresTo(connectionToClient);
        }

    }
}