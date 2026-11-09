using UnityEngine;

public class EnemyFootsteps : MonoBehaviour
{
    [Header("Footstep Audio")]
    public AudioClip footstepSFX;
    public float volume = 1f;

    [Header("Random Variation")]
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    public AudioSource audioSource;

    private void Awake()
    {
        

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    public void PlayFootstep()
    {
        if (footstepSFX == null || audioSource == null)
            return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.PlayOneShot(footstepSFX, volume);

        audioSource.pitch = 1f;
    }
}