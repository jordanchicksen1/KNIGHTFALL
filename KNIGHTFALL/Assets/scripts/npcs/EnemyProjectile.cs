using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 15f;

    public int damage = 15;

    public float lifeTime = 5f;

    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        transform.position +=
            moveDirection *
            speed *
            Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // SHIELD BLOCK
        if (other.CompareTag("Shield"))
        {
            PlayerCombat combat =
                other.GetComponentInParent<PlayerCombat>();

            PlayerHealth playerHealth =
                other.GetComponentInParent<PlayerHealth>();

            if (combat != null &&
                combat.IsBlocking())
            {
                playerHealth.stamina -= 15f;

                playerHealth.ResetStaminaRegenDelay();

                Destroy(gameObject);
            }

            return;
        }

        // PLAYER HIT
        PlayerHealth player =
            other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            Vector3 hitDirection =
                (
                    other.transform.position -
                    transform.position
                ).normalized;

            player.TakeDamage(
                damage,
                hitDirection
            );

            Destroy(gameObject);
        }
    }
}