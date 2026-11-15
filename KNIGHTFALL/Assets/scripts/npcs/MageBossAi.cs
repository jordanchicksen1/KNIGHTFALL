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
    private bool castProjectileFired;
    private int projectilesRemaining;
    private bool isCastingBurst;

    private Rigidbody rb;
    private EnemyHealth enemyHealth;
    private Animator bossAnimator;

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
    private bool isMoving;
    private bool isHitReacting;
    private Coroutine hitReactionCoroutine;
    public float decisionCooldown = 0.2f;
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
        bossAnimator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponentInChildren<EnemyHealth>();
        if (bossAnimator == null)
        {
            Debug.LogWarning("The Overseer is missing an Animator!");
        }

        
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

        
        HandleCrossbowVisual();

        if (!isAttacking && !isDodging && !isMoving && !isMakingDecision && !isHitReacting)
        {
            StartCoroutine(MakeCombatDecision());
        }
    }

   

   

    public void AnimationFireProjectile()
    {
        if (!isCastingBurst || castProjectileFired)
            return;

        castProjectileFired = true;
        FireProjectile();

        projectilesRemaining--;
    }

    void SetMovementAnimation(Vector3 worldDirection)
    {
        if (bossAnimator == null)
            return;

        worldDirection.y = 0f;

        if (worldDirection.sqrMagnitude > 0.01f)
            worldDirection.Normalize();

        Vector3 localDirection =
            transform.InverseTransformDirection(worldDirection);

        bossAnimator.SetFloat("MoveX", localDirection.x, 0.2f, Time.deltaTime);
        bossAnimator.SetFloat("MoveY", localDirection.z, 0.2f, Time.deltaTime);
    }

    void ResetMovementAnimation()
    {
        if (bossAnimator == null)
            return;

        bossAnimator.SetFloat("MoveX", 0f, 0.2f, Time.deltaTime);
        bossAnimator.SetFloat("MoveY", 0f, 0.2f, Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (player == null || enemyHealth == null || enemyHealth.health <= 0)
            return;

        if (aggroTimer <= 0)
            return;

        Vector3 direction = player.position - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            )
        );
    }

    void HandleCrossbowVisual()
    {
        Quaternion targetRotation = isAttacking ? aimingRotation : relaxedRotation;
        crossbowHolder.localRotation = Quaternion.Slerp(crossbowHolder.localRotation, targetRotation, 12f * Time.deltaTime);
    }


    IEnumerator MakeCombatDecision()
    {
        isMakingDecision = true;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        summonedSkeletons.RemoveAll(skeleton => skeleton == null);

        // Summon if the cooldown has finished and there is room.
        if (summonTimer >= summonCooldown &&
            summonedSkeletons.Count < maxSummons)
        {
            yield return StartCoroutine(SummonSkeletons());
            summonTimer = 0f;
        }

        int decision = Random.Range(0, 100);

        // VERY FAR: Walk towards the player.
        if (distance > chaseDistance)
        {
            yield return StartCoroutine(WalkTowardsPlayer(1.5f));
        }

        // FAR: Attack while occasionally closing the distance.
        else if (distance > preferredCombatDistance + 2f)
        {
            if (decision < 50)
                yield return StartCoroutine(WalkTowardsPlayer(1.2f));
            else if (decision < 75)
                yield return StartCoroutine(RandomBurstShot());
            else
                yield return StartCoroutine(Strafe());
        }

        // IDEAL COMBAT DISTANCE: Prioritise attacking.
        else if (distance >= retreatDistance + 1f)
        {
            if (decision < 65)
                yield return StartCoroutine(RandomBurstShot());
            else if (decision < 85)
                yield return StartCoroutine(Strafe());
            else if (decision < 95)
                yield return StartCoroutine(WalkTowardsPlayer(0.8f));
            else
                yield return StartCoroutine(WalkAwayFromPlayer(0.8f));
        }

        // TOO CLOSE: Create space, but still attack occasionally.
        else
        {
            if (decision < 50)
                yield return StartCoroutine(WalkAwayFromPlayer(1.2f));
            else if (decision < 75)
                yield return StartCoroutine(RandomBurstShot());
            else if (decision < 95)
                yield return StartCoroutine(Strafe());
            else
                yield return StartCoroutine(DodgeAway());
        }

        yield return new WaitForSeconds(decisionCooldown);

        isMakingDecision = false;
    }

    

    void MoveBoss(Vector3 direction)
    {
        if (rb == null)
            return;

        direction.y = 0f;
        direction.Normalize();

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime
        );
    }

    IEnumerator WalkTowardsPlayer(float duration)
    {
        isMoving = true;

        float timer = 0f;

        while (timer < duration && !isHitReacting)
        {
            Vector3 direction = player.position - rb.position;
            direction.y = 0f;
            direction.Normalize();

            SetMovementAnimation(direction);

            MoveBoss(direction);

            timer += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        ResetMovementAnimation();
        isMoving = false;
    }

    IEnumerator WalkAwayFromPlayer(float duration)
    {
        isMoving = true;

        float timer = 0f;

        while (timer < duration && !isHitReacting)
        {
            Vector3 direction = rb.position - player.position;
            direction.y = 0f;
            direction.Normalize();

            SetMovementAnimation(direction);

            MoveBoss(direction);

            timer += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        ResetMovementAnimation();
        isMoving = false;
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

    public void PlayHitAnimation()
    {
        if (bossAnimator == null)
            return;

        if (hitReactionCoroutine != null)
            StopCoroutine(hitReactionCoroutine);

        hitReactionCoroutine = StartCoroutine(HitReactionRoutine());
    }

    IEnumerator HitReactionRoutine()
    {
        isHitReacting = true;
        isMoving = false;

        bossAnimator.SetFloat("MoveX", 0f);
        bossAnimator.SetFloat("MoveY", 0f);

        bossAnimator.SetTrigger("Hit");

        // Wait until the Hit animation becomes the active state.
        while (!bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
        {
            if (bossAnimator.IsInTransition(0) &&
                bossAnimator.GetNextAnimatorStateInfo(0).IsName("Hit"))
            {
                break;
            }

            yield return null;
        }

        // Wait for the Hit animation to finish.
        while (true)
        {
            AnimatorStateInfo state = bossAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("Hit") && state.normalizedTime >= 1f)
                break;

            yield return null;
        }

        isHitReacting = false;
        hitReactionCoroutine = null;
    }


    IEnumerator Strafe()
    {
        isMoving = true;

        float duration = Random.Range(1f, 1.6f);
        float direction = Random.Range(0, 2) == 0 ? -1f : 1f;

        float timer = 0f;

        while (timer < duration && !isHitReacting) 
        {
            Vector3 strafeDirection = transform.right * direction;

            SetMovementAnimation(strafeDirection);

            MoveBoss(strafeDirection);

            timer += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        ResetMovementAnimation();
        isMoving = false;
    }






    IEnumerator RandomBurstShot()
    {
        isAttacking = true;
        canAttack = false;
        isCastingBurst = true;

        float healthPercentage =
            enemyHealth.health / enemyHealth.maxHealth;

        // Determine burst size.
        if (healthPercentage > 0.75f)
        {
            int roll = Random.Range(0, 100);
            projectilesRemaining = roll < 55 ? 1 : roll < 90 ? 2 : 3;
        }
        else if (healthPercentage > 0.20f)
        {
            int roll = Random.Range(0, 100);
            projectilesRemaining = roll < 15 ? 1 : roll < 65 ? 2 : 3;
        }
        else
        {
            projectilesRemaining = Random.Range(0, 100) < 25 ? 2 : 3;
        }

        int safetyCounter = 0;
        const float animationTimeout = 4f;

        while (projectilesRemaining > 0 && !isHitReacting)
        {
            castProjectileFired = false;

            bossAnimator.ResetTrigger("CastSpell");
            bossAnimator.SetTrigger("CastSpell");

            // Wait for the casting animation to begin.
            float timer = 0f;

            while (!bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cast Spell"))
            {
                if (isHitReacting || timer >= animationTimeout)
                    break;

                timer += Time.deltaTime;
                yield return null;
            }

            if (isHitReacting || timer >= animationTimeout)
                break;

            // Wait for the casting animation to finish.
            timer = 0f;

            while (bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cast Spell") &&
                   bossAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                if (isHitReacting)
                    break;

                timer += Time.deltaTime;

                if (timer >= animationTimeout)
                    break;

                yield return null;
            }

            if (isHitReacting)
                break;

            // Check that the Animation Event actually fired.
            if (!castProjectileFired)
            {
                Debug.LogError(
                    "The Overseer failed to fire a projectile! " +
                    "Check the castSpell Animation Event."
                );
                break;
            }

            safetyCounter++;

            if (safetyCounter >= 3)
                break;
        }

        isCastingBurst = false;
        projectilesRemaining = 0;

        while (isHitReacting)
            yield return null;

        yield return new WaitForSeconds(attackCooldown);

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