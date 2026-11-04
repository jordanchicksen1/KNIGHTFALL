using UnityEngine;

public class PlayerSpellProjectile : MonoBehaviour
{
    [Header("Spell Stats")]
    public int mpCost = 20;
    public float speed = 20f;
    public int damage = 35;
    public float lifeTime = 5f;
    public SpellType spellType;
    private Vector3 direction;
    public bool isOrbiting;
    private PlayerAudio playerAudio;

    [Header("Status Effects")]
    public int statusBuildup = 20;

    void Start()
    {
        playerAudio = FindFirstObjectByType<PlayerAudio>();
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        transform.forward = direction;
    }

    void Update()
    {
        if (isOrbiting)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOrbiting)
            return;

        // Ignore the player
        if (other.CompareTag("Player"))
            return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            Vector3 hitDirection =
                (other.transform.position - transform.position).normalized;

            enemy.TakeDamage(damage, hitDirection);
            enemy.ApplyStatusBuildup(spellType, statusBuildup);
        }

        // Play impact sound for enemies and environment
        if (playerAudio != null)
            playerAudio.PlaySpellImpact(transform.position);

        Destroy(gameObject);
    }
}