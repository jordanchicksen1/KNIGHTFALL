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

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
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
        // SHIELD COLLISION
        if (other.CompareTag("Shield"))
        {
            PlayerCombat combat = other.GetComponentInParent<PlayerCombat>();
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

            if (combat != null && combat.IsBlocking())
            {
                if (playerHealth != null)
                {
                    Vector3 hitDirection =
                        (other.transform.position - transform.position).normalized;

                    playerHealth.TakeDamage(damage, hitDirection);
                }

                Destroy(gameObject);
            }

            // If the player isn't blocking, let the projectile
            // continue so it can hit the player's body.
            return;
        }

        // PLAYER COLLISION
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            Vector3 hitDirection =
                (other.transform.position - transform.position).normalized;

            player.TakeDamage(damage, hitDirection);

            Destroy(gameObject);
            return;
        }

        // ANY OTHER COLLISION
        Destroy(gameObject);
    }


}