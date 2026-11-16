
using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    public Interactable currentInteractable;
    public EndingStatue activeEndingStatue;
    public TalkingStatue activeTalkingStatue;
    public bool isReadingMessage;

    private PlayerMovement movement;
    private Coroutine interactionCoroutine;
    private bool isInteracting;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    public void TryInteract()
    {
        // Don't allow another interaction while the animation is playing.
        if (isInteracting)
            return;

        if (isReadingMessage)
        {
            if (activeTalkingStatue != null)
            {
                activeTalkingStatue.Interact();
                return;
            }

            if (activeEndingStatue != null)
            {
                activeEndingStatue.Interact();
                return;
            }

            InteractionUI.Instance.HideMessage();
            isReadingMessage = false;
            return;
        }

        if (currentInteractable == null)
            return;

        // Keep a reference to the object in case the player leaves its trigger.
        Interactable interactable = currentInteractable;

        interactionCoroutine = StartCoroutine(
            PerformInteraction(interactable)
        );

        InteractionUI.Instance.HidePrompt();
    }

    private IEnumerator PerformInteraction(Interactable interactable)
    {
        isInteracting = true;

        // Lock player movement.
        movement.movementLocked = true;
        movement.moveInput = Vector2.zero;
        movement.currentState = PlayerState.Attacking;

        // Clear any pending walking animation.
        movement.KnightAnim.ResetTrigger("Walk");

        // Choose the appropriate animation for this interaction.
        string animationState = "Interact";

        if (interactable is Bonfire bonfire && bonfire.IsLit)
        {
            animationState = "Rest";
        }

        movement.KnightAnim.SetTrigger(animationState);

        // Wait until the interaction animation starts.
        while (!movement.KnightAnim.GetCurrentAnimatorStateInfo(0)
            .IsName(animationState))
        {
            yield return null;
        }

        // Wait until the interaction animation finishes.
        // Wait until the interaction animation is nearly finished.
        while (movement.KnightAnim.GetCurrentAnimatorStateInfo(0)
            .normalizedTime < 0.9f)
        {
            yield return null;
        }
        {
            yield return null;
        }

        // Perform the actual interaction.
        if (interactable != null)
        {
            interactable.Interact();
        }

        // Unlock movement.
        movement.movementLocked = false;
        movement.currentState = PlayerState.Idle;

        isInteracting = false;
        interactionCoroutine = null;
    }
}
