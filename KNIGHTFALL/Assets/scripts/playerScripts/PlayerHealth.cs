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

    private PlayerCombat combat;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        combat = GetComponent<PlayerCombat>();
    }

    public void TakeDamage(int damage,Vector3 hitDirection)
    {
        if (combat.IsBlocking())
        {
            Debug.Log("Blocked Attack");

            return;
        }

        health -= damage;

        StopAllCoroutines();

        StartCoroutine(Knockback(hitDirection));

        Debug.Log("Player took damage");

        if (health <= 0)
        {
            Debug.Log("Player Died");
        }
    }

    IEnumerator Knockback(Vector3 hitDirection)
    {
        movement.currentState = PlayerState.Staggered;

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

        movement.currentState = PlayerState.Idle;
    }

    

    IEnumerator Stagger()
    {
        movement.currentState = PlayerState.Staggered;

        yield return new WaitForSeconds(staggerDuration);

        movement.currentState = PlayerState.Idle;
    }
}