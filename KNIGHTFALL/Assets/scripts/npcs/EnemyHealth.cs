using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;

    public float knockbackForce = 5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        health -= damage;

        rb.AddForce(
            hitDirection * knockbackForce,
            ForceMode.Impulse
        );

        Debug.Log(gameObject.name + " took damage.");

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}