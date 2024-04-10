using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RadioNoiseController : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;
    private ItemSwitch itemSwitch;
    public string ghostTag = "Ghost"; // Tag to identify the ghost GameObject
    private Transform ghostTransform;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        itemSwitch = GetComponentInParent<ItemSwitch>(); // Assuming the player has the ItemSwitch component
    }

    void Update()
    {
        // Find the ghost in the scene by tag
        GameObject ghost = GameObject.FindGameObjectWithTag(ghostTag);
        if (ghost != null)
        {
            ghostTransform = ghost.transform;
        }
        if (ghostTransform == null) return;


        if (itemSwitch == null) return;Debug.Log(itemSwitch.GetActiveItemIndex());

        if (itemSwitch != null && itemSwitch.GetActiveItemIndex() == 2) // Assuming the radio is at index 1
        {
            AdjustVolumeBasedOnDistance();
        }
        else
        {
            // Mute the radio if it's not the active item
            audioSource.volume = 0;
        }
    }

    private void AdjustVolumeBasedOnDistance()
    {
        float distance = Vector3.Distance(transform.position, ghostTransform.position);
        float maxVolumeDistance = 5f; // Distance at which the volume is at its highest
        float minVolumeDistance = 20f; // Distance at which volume starts to decrease

        // Normalize the distance within our volume range and invert it
        float volume = 1.0f - Mathf.Clamp01((distance - maxVolumeDistance) / (minVolumeDistance - maxVolumeDistance));

        audioSource.volume = volume;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }
}
