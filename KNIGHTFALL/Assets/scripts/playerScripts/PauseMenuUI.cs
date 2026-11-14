
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Controller")]
    public PauseMenu pauseMenu;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;

    [Header("Main Menu")]
    public Button resumeButton;
    public Button inventoryButton;
    public Button controlsButton;
    public Button quitButton;

    [Header("Pages")]
    public GameObject inventoryPage;
    public GameObject controlsPage;
    public GameObject quitPage;

    [Header("Inventory")]
    public Button swordButton;

    [Header("Player Inventory")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Unlockable Inventory Buttons")]
    [SerializeField] private GameObject staffButton;
    [SerializeField] private GameObject fireballButton;
    [SerializeField] private GameObject iceSpearButton;
    [SerializeField] private GameObject lightningBoltButton;
    [SerializeField] private GameObject keyButton;

    [Header("Page Close Buttons")]
    public Button inventoryCloseButton;
    public Button controlsCloseButton;

    [Header("Quit Confirmation")]
    public Button yesButton;
    public Button noButton;

    private void Awake()
    {
        // Connect main menu buttons.
        resumeButton.onClick.AddListener(ResumeGame);
        inventoryButton.onClick.AddListener(OpenInventory);
        controlsButton.onClick.AddListener(OpenControls);
        quitButton.onClick.AddListener(OpenQuitConfirmation);

        // Connect page close buttons.
        inventoryCloseButton.onClick.AddListener(ClosePage);
        controlsCloseButton.onClick.AddListener(ClosePage);

        // Connect quit confirmation buttons.
        yesButton.onClick.AddListener(QuitGame);
        noButton.onClick.AddListener(ClosePage);

        // Make sure all pages start closed.
        CloseAllPages();
    }

    // --------------------------------------------------
    // OPEN MENU
    // --------------------------------------------------

    public void OpenMenu()
    {
        pauseMenuPanel.SetActive(true);

        CloseAllPages();

        SelectButton(resumeButton);
    }

    // --------------------------------------------------
    // MAIN MENU OPTIONS
    // --------------------------------------------------

    public void ResumeGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.ResumeGame();
        }
    }

    private void OpenInventory()
    {
        CloseAllPages();

        UpdateInventoryVisibility();

        inventoryPage.SetActive(true);

        StartCoroutine(SelectInventoryButton());
    }

    private void UpdateInventoryVisibility()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory reference is missing!");
            return;
        }

        // Weapons
        staffButton.SetActive(playerInventory.HasWeapon(WeaponType.Staff));

        // Spells
        fireballButton.SetActive(playerInventory.HasSpell(SpellType.Fireball));
        iceSpearButton.SetActive(playerInventory.HasSpell(SpellType.IceSpear));
        lightningBoltButton.SetActive(playerInventory.HasSpell(SpellType.LightningBolt));

        keyButton.SetActive(playerInventory.keys > 0);
    }

    private IEnumerator SelectInventoryButton()
    {
        // Wait for the inventory page to become active.
        yield return null;

        if (EventSystem.current == null || swordButton == null)
            yield break;

        // Clear the previous selection.
        EventSystem.current.SetSelectedGameObject(null);

        // Explicitly select the sword button.
        EventSystem.current.SetSelectedGameObject(swordButton.gameObject);
    }

    private void OpenControls()
    {
        CloseAllPages();

        controlsPage.SetActive(true);

        SelectButton(controlsCloseButton);
    }

    private void OpenQuitConfirmation()
    {
        CloseAllPages();

        quitPage.SetActive(true);

        // Select No by default to avoid accidental quitting.
        SelectButton(noButton);
    }

    // --------------------------------------------------
    // CLOSE PAGES
    // --------------------------------------------------

    public void ClosePage()
    {
        CloseAllPages();

        // Return controller selection to Resume.
        SelectButton(resumeButton);
    }

    private void CloseAllPages()
    {
        if (inventoryPage != null)
            inventoryPage.SetActive(false);

        if (controlsPage != null)
            controlsPage.SetActive(false);

        if (quitPage != null)
            quitPage.SetActive(false);
    }

    public void ClosePauseMenu()
    {
        pauseMenuPanel.SetActive(false);
    }

    // --------------------------------------------------
    // QUIT GAME
    // --------------------------------------------------

    private void QuitGame()
    {
        Time.timeScale = 1f;

        Debug.Log("Quitting Knightfall.");

        Application.Quit();
    }

    // --------------------------------------------------
    // CONTROLLER SELECTION
    // --------------------------------------------------

    private void SelectButton(Button button)
    {
        if (button == null || EventSystem.current == null)
            return;

        StartCoroutine(SelectButtonNextFrame(button));
    }

    private IEnumerator SelectButtonNextFrame(Button button)
    {
        yield return null;

        if (button == null || !button.gameObject.activeInHierarchy)
            yield break;

        if (EventSystem.current == null)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
