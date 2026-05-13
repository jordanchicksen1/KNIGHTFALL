using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;

    public float staggerDuration = 0.25f;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (movement.currentState == PlayerState.Attacking)
        {
            StopAllCoroutines();

            StartCoroutine(Stagger());
        }

        Debug.Log("Player took damage");

        if (health <= 0)
        {
            Debug.Log("Player Died");
        }
    }

    IEnumerator Stagger()
    {
        movement.currentState = PlayerState.Staggered;

        yield return new WaitForSeconds(staggerDuration);

        movement.currentState = PlayerState.Idle;
    }
}