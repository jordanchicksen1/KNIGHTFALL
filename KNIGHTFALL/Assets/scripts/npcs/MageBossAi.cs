using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MageBossAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform firePoint;
    public BoxCollider arenaBounds;

    [Header("Magic Projectiles")]
    public GameObject fireProjectilePrefab;
    public GameObject iceProjectilePrefab;
    public GameObject lightningProjectilePrefab;
    private bool castProjectileFired;
    private int projectilesRemaining;
    private bool isCastingBurst;

    [Header("Dodge Visuals")]
    public Transform bossVisualRoot;
    private Renderer[] bossRenderers;
    public TrailRenderer dodgeTrail;
    public Light dodgeTrailLight;

    private Rigidbody rb;
    private EnemyHealth enemyHealth;
    private bool deathSequenceFinished;
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

    [Header("Boss Music and SFX")]
    public AudioSource bossMusic;
    private bool isBossMusicPlaying;
    private MageBossAudio bossAudio;


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
    private bool isDead;

    [Header("Hit Retaliation")]
    public int retaliationHitThreshold = 3;
    public float retaliationHitWindow = 2.5f;
    public float retaliationCooldown = 8f;

    private List<float> recentHitTimes = new List<float>();
    private float nextRetaliationTime;
    private bool isRetaliating;
    private bool retaliationPending;


    [Header("Summoning")]
    public GameObject[] summonPrefabs;
    public Transform[] summonPoints;
    public int maxSummons = 3;
    public float summonCooldown = 10f;
    private float summonTimer;
    private List<GameObject> summonedSkeletons = new List<GameObject>();
    public GameObject summonSignPrefab;
    public float summonRaycastHeight = 5f;
    public float summonRaycastDistance = 15f;
    public LayerMask groundLayer;

    
    [Header("Fake Projectiles")]
    public Transform fakeProjectileHolder;

    public GameObject fakeFireProjectilePrefab;
    public GameObject fakeIceProjectilePrefab;
    public GameObject fakeLightningProjectilePrefab;

    private GameObject currentFakeProjectile;
    private bool fakeProjectileHiddenForCast;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bossAnimator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponentInChildren<EnemyHealth>();
        bossAudio = GetComponent<MageBossAudio>();

        if (bossAnimator == null)
        {
            Debug.LogWarning("The Overseer is missing an Animator!");
        }

        if (bossVisualRoot != null)
        {
            Renderer[] allRenderers =
                bossVisualRoot.GetComponentsInChildren<Renderer>(true);

            List<Renderer> visualRenderers = new List<Renderer>();

            foreach (Renderer rend in allRenderers)
            {
                if (rend != dodgeTrail)
                {
                    visualRenderers.Add(rend);
                }
            }

            bossRenderers = visualRenderers.ToArray();
        }


        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

       
        startPosition = transform.position;
        startRotation = transform.rotation;

    }

    void Update()
    {
       

        summonTimer += Time.deltaTime;

        UpdateFakeProjectile();

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
            StopBossMusic();
            return;
        }

        // Player is still detected or within aggro memory.
        StartBossMusic();


       

        if (!isAttacking && !isDodging && !isMoving && !isMakingDecision && !isHitReacting)
        {
            StartCoroutine(MakeCombatDecision());
        }
    }

    void UpdateFakeProjectile()
    {
        if (fakeProjectileHolder == null || enemyHealth == null)
            return;

        if (fakeProjectileHiddenForCast)
            return;

        float healthPercentage =
            (float)enemyHealth.health / enemyHealth.maxHealth;

        GameObject selectedFakeProjectile = null;

        if (healthPercentage > 0.75f)
            selectedFakeProjectile = fakeFireProjectilePrefab;
        else if (healthPercentage > 0.20f)
            selectedFakeProjectile = fakeIceProjectilePrefab;
        else
            selectedFakeProjectile = fakeLightningProjectilePrefab;

        if (selectedFakeProjectile == null)
            return;

        // If the correct projectile already exists, make sure it is visible.
        if (currentFakeProjectile != null &&
            currentFakeProjectile.name.StartsWith(selectedFakeProjectile.name))
        {
            currentFakeProjectile.SetActive(true);
            return;
        }

        // Remove the previous magic projectile.
        if (currentFakeProjectile != null)
            Destroy(currentFakeProjectile);

        // Create the new one.
        currentFakeProjectile = Instantiate(
            selectedFakeProjectile,
            fakeProjectileHolder
        );

        currentFakeProjectile.transform.localPosition = Vector3.zero;
        
        
    }

    void HideFakeProjectile()
    {
        fakeProjectileHiddenForCast = true;

        if (currentFakeProjectile != null)
            currentFakeProjectile.SetActive(false);
    }

    void ShowFakeProjectile()
    {
        fakeProjectileHiddenForCast = false;
        UpdateFakeProjectile();
    }

    void HideBossVisuals()
    {
        if (bossRenderers == null)
            return;

        foreach (Renderer rend in bossRenderers)
        {
            rend.enabled = false;
        }
    }

    void ShowBossVisuals()
    {
        if (bossRenderers == null)
            return;

        foreach (Renderer rend in bossRenderers)
        {
            rend.enabled = true;
        }
    }

    public void RegisterHitTaken()
    {
        if (isDead || enemyHealth.health <= 0)
            return;

        float currentTime = Time.time;

        // Remove hits outside the retaliation window.
        recentHitTimes.RemoveAll(
            hitTime => currentTime - hitTime > retaliationHitWindow
        );

        recentHitTimes.Add(currentTime);

        if (recentHitTimes.Count < retaliationHitThreshold)
            return;

        recentHitTimes.Clear();

        // Don't retaliate again while on cooldown.
        if (currentTime < nextRetaliationTime)
            return;

        // Queue retaliation if currently casting.
        if (isCastingBurst || IsCastingSpellAnimation())
        {
            retaliationPending = true;
            return;
        }

        BeginRetaliation();
    }

    void BeginRetaliation()
    {
        if (isDead || isRetaliating)
            return;

        // Never interrupt a spell cast.
        if (isCastingBurst || IsCastingSpellAnimation())
        {
            retaliationPending = true;
            return;
        }

        retaliationPending = false;
        nextRetaliationTime = Time.time + retaliationCooldown;

        // Interrupt the current combat behaviour.
        StopAllCoroutines();

        isHitReacting = false;
        isMoving = false;
        isDodging = false;
        isMakingDecision = false;

        isCastingBurst = false;
        projectilesRemaining = 0;

        if (bossAnimator != null)
        {
            bossAnimator.ResetTrigger("Hit");
            bossAnimator.SetFloat("MoveX", 0f);
            bossAnimator.SetFloat("MoveY", 0f);
        }

        StartCoroutine(RetaliationRoutine());
    }

    IEnumerator RetaliationRoutine()
    {
        isRetaliating = true;
        isAttacking = true;
        canAttack = false;

        // First, create some distance.
        yield return StartCoroutine(DodgeAway());

        // Then immediately fire a guaranteed three-shot burst.
        yield return StartCoroutine(RandomBurstShot(3));

        isRetaliating = false;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        StopBossMusic();

        // Stop all active combat routines.
        StopAllCoroutines();

        // Stop combat behaviour.
        isAttacking = false;
        isDodging = false;
        isMoving = false;
        isMakingDecision = false;
        isCastingBurst = false;
        canAttack = false;

        // Stop movement animations.
        if (bossAnimator != null)
        {
            bossAnimator.SetFloat("MoveX", 0f);
            bossAnimator.SetFloat("MoveY", 0f);
            bossAnimator.SetTrigger("Dying");

            if (bossAudio != null)
                bossAudio.PlayDeath();
        }

        // Remove fake projectile.
        if (currentFakeProjectile != null)
            Destroy(currentFakeProjectile);

        // Remove summoned enemies.
        KillSummons(); 
        SetAllSummonMarkers(false);
        
    }

    public void DeathAnimationFinished()
    {
        if (deathSequenceFinished)
            return;

        deathSequenceFinished = true;

        FinishDeathSequence();
    }
    private void FinishDeathSequence()
    {
        // Play the death effect.
        enemyHealth.PlayDeathEffect();

        // Disable physics and collisions.
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Disable all colliders on the boss.
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Trigger the victory announcement and fanfare.
        if (InteractionUI.Instance != null)
            InteractionUI.Instance.ShowGreatEnemyDefeated();

        // Finally, disable the boss.
        gameObject.SetActive(false);
    }

    public void AnimationFireProjectile()
    {
        if (!isCastingBurst || castProjectileFired)
            return;

        castProjectileFired = true;

        HideFakeProjectile();

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

    IEnumerator ResetMovementAnimation()
    {
        if (bossAnimator == null)
            yield break;

        float elapsed = 0f;
        float blendDuration = 0.25f;

        while (elapsed < blendDuration)
        {
            bossAnimator.SetFloat("MoveX", 0f, 0.2f, Time.deltaTime);
            bossAnimator.SetFloat("MoveY", 0f, 0.2f, Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the values finish at zero.
        bossAnimator.SetFloat("MoveX", 0f);
        bossAnimator.SetFloat("MoveY", 0f);
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

        Vector3 targetPosition =
            rb.position + direction * moveSpeed * Time.fixedDeltaTime;

        targetPosition = ClampToArena(targetPosition);

        rb.MovePosition(targetPosition);
    }

    Vector3 ClampToArena(Vector3 targetPosition)
    {
        if (arenaBounds == null)
            return targetPosition;

        Bounds bounds = arenaBounds.bounds;

        // Keep a little distance from the actual edge.
        float padding = 0.5f;

        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            bounds.min.x + padding,
            bounds.max.x - padding
        );

        targetPosition.z = Mathf.Clamp(
            targetPosition.z,
            bounds.min.z + padding,
            bounds.max.z - padding
        );

        return targetPosition;
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

        if (summonedSkeletons.Count >= maxSummons)
            yield break;

        if (summonPrefabs == null || summonPrefabs.Length == 0)
        {
            Debug.LogWarning("The Overseer has no summon prefabs assigned!");
            yield break;
        }

        if (summonPoints == null || summonPoints.Length == 0)
        {
            Debug.LogWarning("The Overseer has no summon points assigned!");
            yield break;
        }

        // Select a random enemy and spawn point.
        GameObject selectedPrefab =
            summonPrefabs[Random.Range(0, summonPrefabs.Length)];

        Transform selectedPoint =
            summonPoints[Random.Range(0, summonPoints.Length)];

        if (selectedPrefab == null || selectedPoint == null)
            yield break;

        // Find the ground below the selected summon point.
        Vector3 rayOrigin =
            selectedPoint.position + Vector3.up * summonRaycastHeight;

        bool foundGround = Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            summonRaycastDistance,
            groundLayer
        );

        // Stop the Overseer moving and prepare to summon.
        isAttacking = true;
        canAttack = false;
        isMoving = false;

        if (bossAnimator != null)
        {
            bossAnimator.SetFloat("MoveX", 0f);
            bossAnimator.SetFloat("MoveY", 0f);

            bossAnimator.ResetTrigger("Summon");
            bossAnimator.SetTrigger("Summon");
            if (bossAudio != null)
                bossAudio.PlaySummon();
        }

        // Wait for the Summon animation to become active.
        while (!bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Summon"))
        {
            if (isDead)
                yield break;

            yield return null;
        }

        // Create the summon sign on the ground.
        GameObject summonSign = null;

        if (foundGround && summonSignPrefab != null)
        {
            Quaternion signRotation =
                Quaternion.FromToRotation(Vector3.up, hit.normal) *
                summonSignPrefab.transform.rotation;

            Vector3 signPosition = hit.point + hit.normal * 0.075f;

            summonSign = Instantiate(
                summonSignPrefab,
                signPosition,
                signRotation
            );
        }
        else
        {
            Debug.LogWarning(
                "Could not place summon sign at " + selectedPoint.name
            );
        }

        // Wait for the Summon animation to finish.
        while (bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Summon") &&
               bossAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            if (isDead)
            {
                if (summonSign != null)
                    Destroy(summonSign);

                yield break;
            }

            yield return null;
        }

        Vector3 enemySpawnPosition = selectedPoint.position + Vector3.down * 0.75f;

        GameObject skeleton = Instantiate(
            selectedPrefab,
            enemySpawnPosition,
            selectedPoint.rotation
        );

        summonedSkeletons.Add(skeleton);

        // Play the spawn effect using the existing effect pool.
        EnemyDeathEffectPool effectPool =
            FindFirstObjectByType<EnemyDeathEffectPool>();

        if (effectPool != null)
            effectPool.PlaySpawnEffect(selectedPoint.position);
        else
            Debug.LogWarning("EnemyDeathEffectPool not found!");

        // Remove the summon sign.
        if (summonSign != null)
            Destroy(summonSign);

        Debug.Log("The Overseer summoned: " + selectedPrefab.name);

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        canAttack = true;
    }






    void SetAllSummonMarkers(bool active)
    {
        if (summonPoints == null)
            return;

        foreach (Transform point in summonPoints)
        {
            if (point == null)
                continue;

            Transform marker = point.Find("SpawnMarker");

            if (marker != null)
                marker.gameObject.SetActive(active);
        }
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
        StopBossMusic();

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
        SetAllSummonMarkers(false);

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
        if (bossAnimator == null || isDead || isRetaliating)
            return;

        // Don't interrupt an existing hit reaction.
        if (isHitReacting)
            return;

        // Hyperarmour while casting.
        if (isCastingBurst || IsCastingSpellAnimation())
            return;

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

    bool IsCastingSpellAnimation()
    {
        AnimatorStateInfo currentState =
            bossAnimator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName("Cast Spell"))
            return true;

        if (bossAnimator.IsInTransition(0))
        {
            AnimatorStateInfo nextState =
                bossAnimator.GetNextAnimatorStateInfo(0);

            if (nextState.IsName("Cast Spell"))
                return true;
        }

        return false;
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



    IEnumerator RandomBurstShot(int forcedProjectileCount = 0)
    {
        isAttacking = true;
        canAttack = false;
        isCastingBurst = true;

        StartCoroutine(ResetMovementAnimation());

        float healthPercentage = enemyHealth.health / enemyHealth.maxHealth;

        // Determine burst size.
        // Use a forced burst size when specified.
        if (forcedProjectileCount > 0)
        {
            projectilesRemaining = forcedProjectileCount;
        }
        else if (healthPercentage > 0.75f)
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

        const float animationTimeout = 4f;

        while (projectilesRemaining > 0 && !isHitReacting)
        {
            float timer = 0f;

            // IMPORTANT: Wait until the PREVIOUS cast has fully exited.
            while (bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cast Spell"))
            {
                timer += Time.deltaTime;

                if (timer >= animationTimeout)
                {
                    Debug.LogError("The Overseer timed out waiting for the previous cast to finish.");
                    break;
                }

                yield return null;
            }

            if (timer >= animationTimeout || isHitReacting)
                break;

            // Prepare the next projectile.
            castProjectileFired = false;

            bossAnimator.ResetTrigger("CastSpell");
            bossAnimator.SetTrigger("CastSpell");
            if (bossAudio != null)
                bossAudio.PlaySpellCast();

            // Wait for the NEW casting animation to begin.
            timer = 0f;

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

            if (isHitReacting || timer >= animationTimeout)
                break;

            // Confirm the Animation Event fired.
            if (!castProjectileFired)
            {
                Debug.LogError(
                    "The Overseer failed to fire a projectile! " +
                    "Check the castSpell Animation Event."
                );

                break;
            }

            
            ShowFakeProjectile();

            projectilesRemaining--;
        }

        isCastingBurst = false;
        projectilesRemaining = 0;

        ShowFakeProjectile();

        while (isHitReacting)
            yield return null;

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;

        // If retaliation was queued during a spell cast, perform it now.
        if (retaliationPending)
        {
            BeginRetaliation();
        }
    }


    void StartBossMusic()
    {
        if (bossMusic == null || isBossMusicPlaying)
            return;

        bossMusic.Play();
        isBossMusicPlaying = true;
    }

    void StopBossMusic()
    {
        if (bossMusic == null || !isBossMusicPlaying)
            return;

        bossMusic.Stop();
        isBossMusicPlaying = false;
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

    public bool IsDodging()
    {
        return isDodging;
    }
    IEnumerator DodgeAway()
    {
        // Begin dodge.
        isDodging = true;

        // Start the dodge trail.
        if (dodgeTrail != null)
        {
            dodgeTrail.gameObject.SetActive(true);
            dodgeTrail.Clear();
            dodgeTrail.emitting = true;
        }

        if (dodgeTrailLight != null)
        {
            dodgeTrailLight.enabled = true;
        }

        // Hide the boss's visual model.
        HideBossVisuals();
        HideFakeProjectile();

        // Break the player's lock-on.
        PlayerLockOn playerLockOn = player.GetComponent<PlayerLockOn>();

        if (playerLockOn != null)
        {
            playerLockOn.ClearTarget();
        }

        // Calculate dodge direction.
        Vector3 dodgeDirection =
            (transform.position - player.position).normalized;

        dodgeDirection += transform.right * Random.Range(-1.5f, 1.5f);

        dodgeDirection.y = 0;
        dodgeDirection.Normalize();

        Vector3 startPosition = transform.position;

        float dodgeDistance = 5f;
        float duration = 0.55f;
        float timer = 0;

        // Move the boss.
        while (timer < duration)
        {
            float progress = timer / duration;

            Vector3 targetPosition =
                startPosition + dodgeDirection * dodgeDistance;

            Vector3 dodgePosition =
                Vector3.Lerp(startPosition, targetPosition, progress);

            rb.MovePosition(ClampToArena(dodgePosition));

            timer += Time.deltaTime;

            yield return null;
        }

        // Stop creating new trail points.
        // Leave the object active so the existing trail can fade.
        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = false;

        }

        if (dodgeTrailLight != null)
        {
            dodgeTrailLight.enabled = false;
        }

        // Make the boss visible again.
        ShowBossVisuals();
        ShowFakeProjectile();

        // End dodge and restore vulnerability.
        isDodging = false;
    }
}