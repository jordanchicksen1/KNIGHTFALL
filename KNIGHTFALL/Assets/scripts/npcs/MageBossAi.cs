using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MageBossAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform firePoint;

    [Header("Magic Projectiles")]
    public GameObject fireProjectilePrefab;
    public GameObject iceProjectilePrefab;
    public GameObject lightningProjectilePrefab;

    private Rigidbody rb;
    private EnemyHealth enemyHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float preferredDistance = 8f;
    public float retreatDistance = 4f;
    public float detectionRange = 10f;
    public float chaseDistance = 12f;
    public float preferredCombatDistance = 8f;
    private Vector3 startPosition;
    private Quaternion startRotation;

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

    [Header("Summoning")]
    public GameObject[] summonPrefabs;
    public Transform[] summonPoints;
    public int maxSummons = 3;
    public float summonCooldown = 10f;
    private float summonTimer;
    private List<GameObject> summonedSkeletons = new List<GameObject>();

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
        startPosition = transform.position;
        startRotation = transform.rotation;

    }

    void Update()
    {
        summonTimer += Time.deltaTime;

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
        crossbowHolder.localRotation = Quaternion.Slerp(crossbowHolder.localRotation, targetRotation, 12f * Time.deltaTime);
    }


    IEnumerator MakeCombatDecision()
    {
        isMakingDecision = true;
        float distance = Vector3.Distance(transform.position, player.position);
        int decision = Random.Range(0, 100);

        summonedSkeletons.RemoveAll(skeleton => skeleton == null);

        if (summonTimer >= summonCooldown &&
            summonedSkeletons.Count < maxSummons)
        {
            yield return StartCoroutine(SummonSkeletons());

            summonTimer = 0f;
        }

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

                // FIVE BURST SHOT
                else if (decision < 65)
                {
                    yield return StartCoroutine(RandomBurstShot());
                }

                // BURST SHOT
                else
                {
                    yield return StartCoroutine(RandomBurstShot());
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
                    yield return StartCoroutine(RandomBurstShot());
                }
                else
                {
                    yield return StartCoroutine(RandomBurstShot());
                }
            }
        }

        yield return new WaitForSeconds(
            decisionCooldown
        );

        isMakingDecision = false;
    }

    IEnumerator SummonSkeletons()
    {
        summonedSkeletons.RemoveAll(skeleton => skeleton == null);

        // Check whether we have room for another summon.
        if (summonedSkeletons.Count >= maxSummons)
            yield break;

        // Check that the prefab array is configured.
        if (summonPrefabs == null || summonPrefabs.Length == 0)
        {
            Debug.LogWarning("The Overseer has no summon prefabs assigned!");
            yield break;
        }

        // Check that summon points are configured.
        if (summonPoints == null || summonPoints.Length == 0)
        {
            Debug.LogWarning("The Overseer has no summon points assigned!");
            yield break;
        }

        isAttacking = true;
        canAttack = false;

        yield return new WaitForSeconds(1f);

        // Select a random enemy.
        int randomPrefabIndex = Random.Range(0, summonPrefabs.Length);
        GameObject selectedPrefab = summonPrefabs[randomPrefabIndex];

        // Select a random spawn point.
        int randomPointIndex = Random.Range(0, summonPoints.Length);
        Transform selectedPoint = summonPoints[randomPointIndex];

        if (selectedPrefab != null && selectedPoint != null)
        {
            GameObject skeleton = Instantiate(
                selectedPrefab,
                selectedPoint.position,
                selectedPoint.rotation
            );

            summonedSkeletons.Add(skeleton);

            Debug.Log("The Overseer summoned: " + selectedPrefab.name);
        }

        yield return new WaitForSeconds(1f);

        isAttacking = false;
        canAttack = true;
    }

    public void KillSummons()
    {
        foreach (GameObject skeleton in summonedSkeletons)
        {
            if (skeleton != null)
            {
                Destroy(skeleton);
            }
        }

        summonedSkeletons.Clear();
    }

    public void ResetBoss()
    {
        // Forget the player
        aggroTimer = 0f;

        // Stop combat
        StopAllCoroutines();

        isAttacking = false;
        isDodging = false;
        isMakingDecision = false;
        canAttack = true;

        // Reset summon timer
        summonTimer = 0f;

        // Kill summons
        KillSummons();

        // Reset transform
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Restore health
        enemyHealth.health = enemyHealth.maxHealth;

        BossHealthUI bossUI = FindFirstObjectByType<BossHealthUI>();

        if (bossUI != null)
        {
            bossUI.HideBoss();
        }
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




    IEnumerator RandomBurstShot()
    {
        isAttacking = true;
        canAttack = false;

        // Brief aiming/wind-up time.
        yield return new WaitForSeconds(0.35f);

        float healthPercentage =
            enemyHealth.health / enemyHealth.maxHealth;

        int projectileCount;

        // PHASE 1: FIRE
        if (healthPercentage > 0.75f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 55)
                projectileCount = 1;
            else if (roll < 90)
                projectileCount = 2;
            else
                projectileCount = 3;
        }
        // PHASE 2: ICE
        else if (healthPercentage > 0.20f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 15)
                projectileCount = 1;
            else if (roll < 65)
                projectileCount = 2;
            else
                projectileCount = 3;
        }
        // PHASE 3: LIGHTNING
        else
        {
            int roll = Random.Range(0, 100);

            if (roll < 25)
                projectileCount = 2;
            else
                projectileCount = 3;
        }

        // Fire the selected number of projectiles.
        for (int i = 0; i < projectileCount; i++)
        {
            FireProjectile();

            if (i < projectileCount - 1)
                yield return new WaitForSeconds(0.18f);
        }

        yield return new WaitForSeconds(attackCooldown + 1f);

        isAttacking = false;
        canAttack = true;
    }



    void FireProjectile()
    {
        if (player == null || firePoint == null)
            return;

        // Calculate the boss's current health percentage.
        float healthPercentage =
            enemyHealth.health / enemyHealth.maxHealth;

        GameObject selectedProjectile = null;

        // PHASE 1: FIRE
        if (healthPercentage > 0.75f)
        {
            selectedProjectile = fireProjectilePrefab;
        }
        // PHASE 2: ICE
        else if (healthPercentage > 0.20f)
        {
            selectedProjectile = iceProjectilePrefab;
        }
        // PHASE 3: LIGHTNING
        else
        {
            selectedProjectile = lightningProjectilePrefab;
        }

        if (selectedProjectile == null)
        {
            Debug.LogWarning("The Overseer is missing a projectile prefab!");
            return;
        }

        // Aim towards the player.
        Vector3 direction =
            (player.position - firePoint.position).normalized;

        // Spawn the selected magical projectile.
        GameObject projectile = Instantiate(
            selectedProjectile,
            firePoint.position,
            Quaternion.identity
        );

        // Set its direction.
        BossProjectile bossProjectile =
            projectile.GetComponent<BossProjectile>();

        if (bossProjectile != null)
        {
            bossProjectile.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning(
                selectedProjectile.name +
                " is missing the BossProjectile component!"
            );
        }
    }


    IEnumerator DodgeAway()
    {
        isDodging = true;
        Vector3 dodgeDirection = (transform.position - player.position).normalized;
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