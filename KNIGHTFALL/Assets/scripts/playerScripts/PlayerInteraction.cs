using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Interactable currentInteractable;


    public bool isReadingMessage;


    public void TryInteract()
    {
        if (isReadingMessage)
        {
            InteractionUI.Instance.HideMessage();
            isReadingMessage = false;
            return;
        }

        if (currentInteractable == null)
            return;

        currentInteractable.Interact();

        InteractionUI.Instance.HidePrompt();
    }
}