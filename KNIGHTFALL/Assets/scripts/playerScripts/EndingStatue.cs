
using UnityEngine;

public class EndingStatue : Interactable
{
    [Header("Ending Dialogue")]
    [TextArea(3, 8)]
    public string dialogue;

    [Header("Scene Transition")]
    public string creditsSceneName = "credits";

    private bool dialogueStarted;
    private bool transitionStarted;

    public override void Interact()
    {
        if (transitionStarted)
            return;

        PlayerInteraction playerInteraction =
            FindFirstObjectByType<PlayerInteraction>();

        if (playerInteraction == null)
            return;

        // First interaction: display the dialogue.
        if (!dialogueStarted)
        {
            dialogueStarted = true;

            playerInteraction.isReadingMessage = true;
            playerInteraction.activeEndingStatue = this;

            InteractionUI.Instance.ShowMessage(dialogue);
            return;
        }

        // Second interaction: close the dialogue and begin the ending.
        transitionStarted = true;

        InteractionUI.Instance.HideMessage();

        playerInteraction.isReadingMessage = false;
        playerInteraction.activeEndingStatue = null;

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeToScene(creditsSceneName);
        }
        else
        {
            Debug.LogError("ScreenFader instance not found!");
        }
    }
}
