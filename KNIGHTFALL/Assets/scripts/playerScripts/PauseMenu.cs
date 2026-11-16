
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PauseMenuUI pauseMenuUI;

    private PlayerMovement playerMovement;
    private PlayerItems playerItems;
    private bool isPaused = false;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerItems = GetComponent<PlayerItems>();
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (isPaused)
            ResumeGame();
        else
            OpenPauseMenu();
    }

    private void OpenPauseMenu()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (playerMovement != null)
            playerMovement.SetMenuInputLocked(true);

        if (playerItems != null)
            playerItems.SetMenuInputLocked(true);

        if (pauseMenuUI != null)
            pauseMenuUI.OpenMenu();
        else
            Debug.LogWarning("PauseMenuUI reference is missing!");
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (playerMovement != null)
            playerMovement.SetMenuInputLocked(false);

        if (playerItems != null)
            playerItems.SetMenuInputLocked(false);

        if (pauseMenuUI != null)
            pauseMenuUI.ClosePauseMenu();
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
