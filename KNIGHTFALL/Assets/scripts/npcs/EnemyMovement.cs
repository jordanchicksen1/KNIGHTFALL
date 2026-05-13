using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 5f;

    [Header("Ranges")]
    public float detectionRange = 10f;
    public float attackRange = 2f;

    [HideInInspector]
    public bool canMove = true;

    private EnemyAttack enemyAttack;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        enemyAttack = GetComponent<EnemyAttack>();
    }

    void Update()
    {
        if (player == null || !canMove)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // Only move if player is detected
        if (distance <= detectionRange)
        {
            RotateTowardsPlayer();

            // Move only if outside attack range
            if (distance > attackRange &&
                !enemyAttack.isAttacking)
            {
                MoveTowardsPlayer();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction =
            (player.position - transform.position).normalized;

        direction.y = 0;

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0;

        Quaternion targetRotation =
            Quaternion.LookRotation(-direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }
}