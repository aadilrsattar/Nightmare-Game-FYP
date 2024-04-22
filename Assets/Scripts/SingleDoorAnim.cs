using UnityEngine;
using Mirror;

public class NetworkedDoorAnim : NetworkBehaviour
{
    [SerializeField] private Animator doorAnimator;
    private bool isOpen = false; 

    void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.CompareTag("Player"))
        {
            isOpen = true;
            RpcToggleDoorState(isOpen);
        }
    }

    [ClientRpc]
    void RpcToggleDoorState(bool newState)
    {
        if (newState)
        {
            doorAnimator.SetTrigger("Open");
        }
    }
}
