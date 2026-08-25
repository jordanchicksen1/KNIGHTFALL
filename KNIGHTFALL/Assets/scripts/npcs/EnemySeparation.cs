using UnityEngine;

public class EnemySeparation : MonoBehaviour
{
    [Header("Spacing")]
    public float separationRadius = 2f;
    public float separationStrength = 2f;

    [Header("Detection")]
    public LayerMask enemyLayer;

    public Vector3 GetSeparationDirection()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(
            transform.position,
            separationRadius,
            enemyLayer
        );

        Vector3 separationDirection = Vector3.zero;
        int enemyCount = 0;

        foreach (Collider enemy in nearbyEnemies)
        {
            if (enemy.transform == transform)
                continue;

            Vector3 directionAway =
                transform.position - enemy.transform.position;

            directionAway.y = 0;

            if (directionAway.magnitude > 0.01f)
            {
                separationDirection += directionAway.normalized;
                enemyCount++;
            }
        }

        if (enemyCount == 0)
            return Vector3.zero;

        return separationDirection.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            separationRadius
        );
    }
}