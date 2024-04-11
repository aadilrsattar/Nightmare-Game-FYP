using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DarkerTrigger : MonoBehaviour
{
    public PostProcessProfile normalProfile;
    public PostProcessProfile darkerProfile;
    private PostProcessVolume volume;
    public float effectDuration = 5f; // Duration of the darker effect in seconds

    private void Start()
    {
        volume = FindObjectOfType<PostProcessVolume>();
        if (volume != null)
        {
            volume.profile = normalProfile;
        }
        else
        {
            Debug.LogWarning("PostProcessVolume not found in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure your player GameObject has the "Player" tag
        {
            StopAllCoroutines(); // Stop any existing coroutines to avoid overlaps
            StartCoroutine(ApplyDarkerEffect());
        }
    }

    IEnumerator ApplyDarkerEffect()
    {
        volume.profile = darkerProfile;
        yield return new WaitForSeconds(effectDuration); // Wait for the effect duration
        volume.profile = normalProfile; // Revert to the normal profile
    }
}
