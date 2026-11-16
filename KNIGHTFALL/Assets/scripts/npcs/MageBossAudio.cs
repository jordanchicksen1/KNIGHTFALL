
using UnityEngine;

public class MageBossAudio : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Spell Casting")]
    public AudioClip spellCastSFX;
    [Range(0f, 1f)]
    public float spellCastVolume = 1f;

    [Header("Summoning")]
    public AudioClip summonSFX;
    [Range(0f, 1f)]
    public float summonVolume = 1f;

    [Header("Death")]
    public AudioClip deathSFX;
    [Range(0f, 1f)]
    public float deathVolume = 1f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    public void PlaySpellCast()
    {
        PlaySFX(spellCastSFX, spellCastVolume);
    }

    public void PlaySummon()
    {
        PlaySFX(summonSFX, summonVolume);
    }

    public void PlayDeath()
    {
        PlaySFX(deathSFX, deathVolume);
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip, volume);
    }
}
