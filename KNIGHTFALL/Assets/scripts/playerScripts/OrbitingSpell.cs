using UnityEngine;

public class OrbitingSpell : MonoBehaviour
{
    private Transform target;
    private float radius;
    private float height;
    private float speed;
    private float angle;
    private PlayerSpellProjectile projectile;

    public void Initialise(
        Transform player,
        float orbitRadius,
        float orbitHeight,
        float orbitSpeed,
        float startAngle)
    {
        target = player;
        projectile = GetComponent<PlayerSpellProjectile>();
        radius = orbitRadius;
        height = orbitHeight;
        speed = orbitSpeed;
        angle = startAngle;
    }

    void Update()
    {
        if (target == null)
            return;

        angle += speed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;

        offset.y = height + Mathf.Sin(Time.time * 3f) * 0.08f;

        transform.position = target.position + offset;
    }

    public void Launch(Vector3 direction)
    {
        projectile.isOrbiting = false;

        projectile.SetDirection(direction);

        Destroy(this);
    }
}