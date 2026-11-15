
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 15f;
    public int damage = 15;
    public float lifeTime = 5f;
    private bool hasHit;

    private Vector3 moveDirection;

    [Header("Elemental Damage")]
    public SpellType spellType = SpellType.Fireball;
    public int statusBuildupAmount = 25;

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
        if (hasHit)
            return;

        // SHIELD COLLISION
        if (other.CompareTag("Shield"))
        {
            PlayerCombat combat =
                other.GetComponentInParent<PlayerCombat>();

            PlayerHealth playerHealth =
                other.GetComponentInParent<PlayerHealth>();

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

            return;
        }

        // PLAYER COLLISION
        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            Vector3 hitDirection =
                (other.transform.position - transform.position).normalized;

            player.TakeDamage(damage, hitDirection);

            if (statusBuildupAmount > 0)
            {
                player.ApplyStatusBuildup(
                    spellType,
                    statusBuildupAmount
                );
            }

            Destroy(gameObject);
            return;
        }

        hasHit = true;
        // OTHER COLLISIONS
        Destroy(gameObject);
    }
}
