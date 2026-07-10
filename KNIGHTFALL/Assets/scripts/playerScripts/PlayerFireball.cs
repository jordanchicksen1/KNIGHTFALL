using UnityEngine;

public class PlayerFireball : MonoBehaviour
{
    public float speed = 20f;

    public int damage = 35;

    public float lifeTime = 5f;

    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position +=
            direction *
            speed *
            Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemy =
            other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            Vector3 hitDirection =
                (
                    other.transform.position -
                    transform.position
                ).normalized;

            enemy.TakeDamage(
                damage,
                hitDirection
            );

            Destroy(gameObject);
        }
    }
}