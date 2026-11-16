
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CreditsMenu : MonoBehaviour
{
    [Header("Menu Button")]
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        StartCoroutine(SelectQuitButton());
    }

    private IEnumerator SelectQuitButton()
    {
        // Wait one frame for the UI to initialise
        yield return null;

        if (EventSystem.current != null && quitButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
