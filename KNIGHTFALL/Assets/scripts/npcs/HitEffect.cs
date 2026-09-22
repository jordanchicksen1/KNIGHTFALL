using UnityEngine;

public class HitEffect : MonoBehaviour
{
    public float lifetime = 0.45f;
    public float speed = 5f;
    public float rotationSpeed = 720f;

    private float timer;
    private Vector3 direction;

    public void Play(Vector3 position, Vector3 hitDirection)
    {
        transform.position = position;

        direction = hitDirection.normalized;

        timer = lifetime;

        gameObject.SetActive(true);
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