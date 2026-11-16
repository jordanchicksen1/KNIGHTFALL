using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Interactable currentInteractable;

    public TalkingStatue activeTalkingStatue;
    public bool isReadingMessage;


    public void TryInteract()
    {
        if (isReadingMessage)
        {
            // Continue statue dialogue if one is active.
            if (activeTalkingStatue != null)
            {
                activeTalkingStatue.Interact();
                return;
            }

            // Otherwise, close the normal message.
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