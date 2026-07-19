using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Interactable currentInteractable;

    public void TryInteract()
    {
        if (currentInteractable == null)
            return;

        currentInteractable.Interact();

        InteractionUI.Instance.HidePrompt();
    }
}