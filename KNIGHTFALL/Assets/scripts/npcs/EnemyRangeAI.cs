using UnityEngine;
using System.Collections;

public class EnemyRangeAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform firePoint;
    public GameObject projectilePrefab;
    private Rigidbody rb;
    private EnemyHealth enemyHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float preferredDistance = 8f;
    public float retreatDistance = 4f;
    public float detectionRange = 10f;
    public float chaseDistance = 12f;
    public float preferredCombatDistance = 8f;

    [Header("Awareness")]
    public float aggroMemoryTime = 4f;
    private float aggroTimer;

    [Header("Combat")]
    public float attackCooldown = 2f;
    public float rotationSpeed = 8f;
    private bool canAttack = true;
    public bool isAttacking;
    private bool isDodging;
    public float decisionCooldown = 0.4f;
    private bool isMakingDecision;

    [Header("Crossbow Visual")]
    public Transform crossbowHolder;
    private Quaternion relaxedRotation;
    private Quaternion aimingRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyHealth = GetComponent<EnemyHealth>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        relaxedRotation = Quaternion.Euler(35f, 0f, 0f);
        aimingRotation = Quaternion.Euler(0f, 0f, 0f);
        crossbowHolder.localRotation = relaxedRotation;

    }

    void Update()
    {
        if (player == null)
            return;

        if (enemyHealth.health <= 0)
            return;

        //AGGRO
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // PLAYER DETECTED
        if (distanceToPlayer <= detectionRange)
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
            return;
        }

        HandleRotation();
        HandleCrossbowVisual();

        if (!isAttacking && !isDodging && !isMakingDecision)
        {
            StartCoroutine(MakeCombatDecision());
        }
    }

    void HandleRotation()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void HandleCrossbowVisual()
    {
        Quaternion targetRotation = isAttacking ? aimingRotation : relaxedRotation;
        crossbowHolder.localRotation = Quaternion.Slerp(crossbowHolder.localRotation,targetRotation, 12f * Time.deltaTime);
    }


    IEnumerator MakeCombatDecision()
    {
        isMakingDecision = true;
        float distance = Vector3.Distance(transform.position, player.position);
        int decision = Random.Range(0, 100);

        // VERY FAR
        if (distance > chaseDistance)
        {
            float timer = 0;

            while (timer < 1f)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
                timer += Time.deltaTime;
                yield return null;
            }
        }


        // TOO CLOSE
        else if (distance < retreatDistance)
        {
            // MOSTLY DODGE
            if (decision < 70)
            {
                yield return StartCoroutine(DodgeAway());
            }
            else
            {
                yield return StartCoroutine(Strafe());
            }
        }

        // COMBAT RANGE
        else
        {
            // PLAYER IS GETTING CLOSE
            if (distance < preferredCombatDistance)
            {
                // DODGE
                if (decision < 20)
                {
                    yield return StartCoroutine(
                        DodgeAway()
                    );
                }

                // STRAFE
                else if (decision < 35)
                {
                    yield return StartCoroutine(
                        Strafe()
                    );
                }

                // SINGLE SHOT
                else if (decision < 65)
                {
                    yield return StartCoroutine(
                        SingleShot()
                    );
                }

                // BURST SHOT
                else
                {
                    yield return StartCoroutine(
                        BurstShot()
                    );
                }
            }

            // PLAYER IS FARTHER AWAY
            else
            {
                if (decision < 25)
                {
                    yield return StartCoroutine(Strafe());
                }
                else if (decision < 40)
                {
                    yield return StartCoroutine(DodgeAway());
                }
                else if (decision < 85)
                {
                    yield return StartCoroutine(
                        SingleShot()
                    );
                }
                else
                {
                    yield return StartCoroutine(BurstShot() );
                }
            }
        }

        yield return new WaitForSeconds(
            decisionCooldown
        );

        isMakingDecision = false;
    }

    IEnumerator Strafe()
    {
        isDodging = true;
        float duration = Random.Range(0.4f, 1f);
        float direction = Random.Range(0, 2) == 0 ? -1 : 1;
        float timer = 0;

        while (timer < duration)
        {
            Vector3 strafeDirection = transform.right * direction;

            rb.MovePosition(transform.position + strafeDirection * moveSpeed * 1.8f * Time.deltaTime);

            timer += Time.deltaTime;

            yield return null;
        }

        isDodging = false;
    }

    IEnumerator SingleShot()
    {
        canAttack = false;
        isAttacking = true;

        yield return new WaitForSeconds(0.35f);

        FireProjectile();

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    IEnumerator BurstShot()
    {
        canAttack = false;
        isAttacking = true;

        yield return new WaitForSeconds(0.35f);

        for (int i = 0; i < 3; i++)
        {
            FireProjectile();

            yield return new WaitForSeconds(0.18f);
        }

        yield return new WaitForSeconds(attackCooldown + 1f);

        isAttacking = false;
        canAttack = true;
    }

    void FireProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector3 direction = (player.position - firePoint.position).normalized;

        projectile.GetComponent<EnemyProjectile>().SetDirection(direction);
    }

    IEnumerator DodgeAway()
    {
        isDodging = true;
        Vector3 dodgeDirection = ( transform.position - player.position).normalized;
        dodgeDirection += transform.right * Random.Range(-1.5f, 1.5f);
        dodgeDirection.y = 0;
        dodgeDirection.Normalize();
        Vector3 startPosition = transform.position;
        float dodgeDistance = 5f;
        float duration = 0.22f;
        float timer = 0;

        while (timer < duration)
        {
            float progress = timer / duration;
            Vector3 targetPosition = startPosition + dodgeDirection * dodgeDistance;
            rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, progress));
            timer += Time.deltaTime;

            yield return null;
        }

        isDodging = false;
    }
}