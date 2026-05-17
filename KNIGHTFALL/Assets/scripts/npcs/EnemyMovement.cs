using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    [Header("Combat")]
    public float detectionRange = 10f;
    public float aggroMemoryTime = 5f;
    private float aggroTimer;
    public float attackRange = 2f;
    public float engageAttackRange = 4f;
    private bool isDeciding;

    public float strafeSpeed = 1.5f;

    public bool canMove = true;

    private EnemyAttack enemyAttack;

    private bool isStrafing;
    private bool isBackingAway;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        enemyAttack = GetComponent<EnemyAttack>();

        StartCoroutine(CombatBehaviour());
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

        bool playerDetected =
            distance <= detectionRange;

        bool remembersPlayer =
            aggroTimer > 0;

        if (playerDetected || remembersPlayer)
        {
            RotateTowardsPlayer();
        }
    }

    IEnumerator CombatBehaviour()
    {
        while (true)
        {
            if (player == null ||
                !canMove ||
                enemyAttack.isAttacking)
            {
                yield return null;
                continue;
            }

            float distance = Vector3.Distance(transform.position, player.position);
           
            // PLAYER DETECTED
            if (distance <= detectionRange)
            {
                aggroTimer = aggroMemoryTime;
            }
            else
            {
                aggroTimer -= Time.deltaTime;
            }

            // LOST PLAYER
            if (aggroTimer <= 0)
            {
                yield return null;

                continue;
            }

            // FAR RANGE → AGGRESSIVE CHASE
            else if (distance > engageAttackRange)
            {
                float chaseTime =
                    Random.Range(0.5f, 1f);

                float timer = 0;

                while (timer < chaseTime)
                {
                    MoveTowardsPlayer();

                    timer += Time.deltaTime;

                    yield return null;
                }
            }

            // MID RANGE
            else if (distance > attackRange)
            {
                int decision =
                    Random.Range(0, 100);

                // LUNGE ATTACK
                if (decision < 60)
                {
                    yield return StartCoroutine(
                        enemyAttack.Attack()
                    );
                }

                // STRAFE PRESSURE
                else
                {
                    yield return StartCoroutine(
                        Strafe()
                    );
                }
            }

            // CLOSE RANGE
            else
            {
                int decision =
                    Random.Range(0, 100);

                // ATTACK
                if (decision < 50)
                {
                    yield return StartCoroutine(
                        enemyAttack.Attack()
                    );
                }

                // STRAFE
                else if (decision < 95)
                {
                    yield return StartCoroutine(
                        Strafe()
                    );
                }

                // BACKSTEP
                else
                {
                    yield return StartCoroutine(
                        BackAway()
                    );
                }
            }

           
            yield return null;
            
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction =
            (player.position - transform.position)
            .normalized;

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
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    IEnumerator Strafe()
    {
        float duration =
            Random.Range(0.3f, 0.6f);

        float direction =
            Random.Range(0, 2) == 0 ? -1 : 1;

        float timer = 0;

        while (timer < duration)
        {
            Vector3 strafeDirection =
                transform.right * direction;

            transform.position +=
                strafeDirection *
                strafeSpeed *
                Time.deltaTime;

            timer += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator BackAway()
    {
        float duration =
            Random.Range(0.2f, 0.45f);

        float timer = 0;

        while (timer < duration)
        {
            Vector3 direction =
                (transform.position - player.position)
                .normalized;

            direction.y = 0;

            transform.position +=
                direction *
                moveSpeed *
                1.2f *
                Time.deltaTime;

            timer += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator ShortRetreat()
    {
        float timer = 0;

        float retreatDuration = 0.25f;

        while (timer < retreatDuration)
        {
            Vector3 direction =
                (transform.position - player.position)
                .normalized;

            direction.y = 0;

            transform.position +=
                direction *
                moveSpeed *
                1.5f *
                Time.deltaTime;

            timer += Time.deltaTime;

            yield return null;
        }
    }
}