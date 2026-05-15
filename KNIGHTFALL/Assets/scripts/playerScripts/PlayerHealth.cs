using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public float maxHealth = 100f;
    public float health = 100f;
    public float staggerDuration = 0.25f;
    private PlayerMovement movement;
    
    [Header("Knockback")]
    public float knockbackDistance = 1f;
    public float knockbackDuration = 0.12f;
    private CharacterController controller;

    [Header("Stamina")]
    public float maxStamina = 100;
    public float stamina = 100;
    public float staminaRegenRate = 20f;
    public float blockingRegenMultiplier = 0.4f;
    private PlayerCombat combat;

    [Header("Stamina Regen Delay")]
    public float staminaRegenDelay = 1f;
    private float staminaTimer;

    [Header("Guard Break")]
    public float guardBreakDuration = 3f;
    private bool isGuardBroken;
    public Transform rightHand;
    public Transform leftHand;
    private Vector3 rightHandStartPos;
    private Vector3 leftHandStartPos;
    private Quaternion rightHandStartRot;
    private Quaternion leftHandStartRot;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        combat = GetComponent<PlayerCombat>();

        rightHandStartPos = rightHand.localPosition;

        leftHandStartPos = leftHand.localPosition;

        rightHandStartRot = rightHand.localRotation;

        leftHandStartRot = leftHand.localRotation;
    }

    void Update()
    {
        RegenerateStamina();
    }

    public void TakeDamage(int damage,Vector3 hitDirection)
    {
        if (combat != null && combat.IsBlocking())
        {
            stamina -= 15f;

            ResetStaminaRegenDelay();

            stamina = Mathf.Max(stamina, 0);

            if (stamina <= 0 &&
                !isGuardBroken)
            {
                StartCoroutine(GuardBreak());

                return;
            }

            Debug.Log("Blocked Attack");

            return;
        }

        health -= damage;

        StartCoroutine(Knockback(hitDirection));

        Debug.Log("Player took damage");

        if (health <= 0)
        {
            Debug.Log("Player Died");
        }
    }

    void RegenerateStamina()
    {
        staminaTimer -= Time.deltaTime;

        if (staminaTimer > 0)
            return;

        if (stamina >= maxStamina)
            return;

        float regenRate = staminaRegenRate;

        if (combat != null &&
            combat.IsBlocking())
        {
            regenRate *=
                blockingRegenMultiplier;
        }

        stamina +=
            regenRate *
            Time.deltaTime;

        stamina =
            Mathf.Clamp(
                stamina,
                0,
                maxStamina
            );
    }

    public void ResetStaminaRegenDelay()
    {
        staminaTimer =
            staminaRegenDelay;
    }

    IEnumerator GuardBreak()
    {
        isGuardBroken = true;

        movement.currentState =
            PlayerState.Staggered;

        // stop current sword attack
        combat.InterruptAttack();

        combat.ResetBlockHold();

        // LOWER HANDS
        rightHand.localPosition =
            rightHandStartPos +
            new Vector3(0, -0.35f, 0);

        leftHand.localPosition =
            leftHandStartPos +
            new Vector3(0, -0.35f, 0);

        rightHand.localRotation =
            Quaternion.Euler(50, 0, 0);

        leftHand.localRotation =
            Quaternion.Euler(50, 0, 0);

        yield return new WaitForSeconds(
            guardBreakDuration
        );

        // RESTORE RIGHT HAND
        rightHand.localPosition =
            rightHandStartPos;

        rightHand.localRotation =
            rightHandStartRot;

        // RESTORE LEFT HAND
        leftHand.localPosition =
            leftHandStartPos;

        leftHand.localRotation =
            leftHandStartRot;

        stamina = 25f;

        movement.currentState =
            PlayerState.Idle;

        isGuardBroken = false;
    }

    IEnumerator Knockback(Vector3 hitDirection)
    {
        if (!isGuardBroken)
        {
            movement.currentState =
                PlayerState.Staggered;
        }

        Vector3 startPosition = transform.position;

        Vector3 targetPosition =
            startPosition +
            (hitDirection * knockbackDistance);

        float timer = 0;

        while (timer < knockbackDuration)
        {
            Vector3 move =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    timer / knockbackDuration
                );

            controller.enabled = false;

            transform.position = move;

            controller.enabled = true;

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(staggerDuration);

        if (!isGuardBroken)
        {
            movement.currentState =
                PlayerState.Idle;
        }
    }


    public bool IsGuardBroken()
    {
        return isGuardBroken;
    }

}