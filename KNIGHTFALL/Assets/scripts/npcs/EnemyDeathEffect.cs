
using UnityEngine;

public class EnemyDeathEffect : MonoBehaviour
{
    [Header("Effect")]
    public float lifetime = 0.6f;
    public float speed = 4f;
    public float rotationSpeed = 720f;

    [Header("Audio")]
    public AudioClip deathSFX;
    public float volume = 1f;

    private float timer;
    private Vector3 direction;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    public void Play(Vector3 position, Vector3 effectDirection)
    {
        transform.position = position;

        direction = effectDirection.normalized;
        timer = lifetime;

        gameObject.SetActive(true);

        if (deathSFX != null)
        {
            audioSource.PlayOneShot(deathSFX, volume);
        }
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        transform.position += direction * speed * Time.deltaTime;

        transform.Rotate(
            Random.insideUnitSphere * rotationSpeed * Time.deltaTime
        );

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}
