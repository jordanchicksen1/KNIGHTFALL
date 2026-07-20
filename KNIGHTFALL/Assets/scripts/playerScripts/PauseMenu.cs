using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel;

    private bool isPaused = false;

    private void Start()
    {
        controlsPanel.SetActive(false);
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        Debug.Log("pause pressed");

        if (!context.performed)
            return;

        isPaused = !isPaused;

        controlsPanel.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}