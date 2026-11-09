using UnityEngine;

public class EnemyNoticeSFX : MonoBehaviour
{
    [Header("Notice Audio")]
    public AudioClip noticeSFX;
    public float volume = 1f;

    public AudioSource audioSource;

    private void Awake()
    {
       

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    public void PlayNoticeSFX()
    {
        if (noticeSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(noticeSFX, volume);
        }
    }
}