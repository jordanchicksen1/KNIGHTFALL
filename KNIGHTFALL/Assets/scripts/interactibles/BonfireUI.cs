using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BonfireUI : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menuPanel;
    public Button leaveButton;
    public Button offerEmberButton;

    private PlayerMovement playerMovement;
    private PlayerItems playerItems;
    private Bonfire activeBonfire;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerItems = player.GetComponent<PlayerItems>();
        }

        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (leaveButton != null)
            leaveButton.onClick.AddListener(Leave);

        if (offerEmberButton != null)
            offerEmberButton.onClick.AddListener(OfferEmber);
    }

    public void Open(Bonfire bonfire)
    {
        if (bonfire == null ||
            playerMovement == null ||
            menuPanel == null)
            return;

        activeBonfire = bonfire;

        // Lock gameplay movement.
        playerMovement.movementLocked = true;

        // Open the menu.
        menuPanel.SetActive(true);

        RefreshMenu();

        // Automatically select Leave.
        if (EventSystem.current != null &&
            leaveButton != null &&
            leaveButton.gameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(
                leaveButton.gameObject
            );
        }
    }

    private void RefreshMenu()
    {
        if (activeBonfire == null ||
            playerItems == null ||
            offerEmberButton == null)
            return;

        bool canOfferEmber =
            !activeBonfire.IsKindled &&
            playerItems.currentEmbers > 0;

        offerEmberButton.gameObject.SetActive(canOfferEmber);
    }

    private void OfferEmber()
    {
        if (activeBonfire == null)
            return;

        if (activeBonfire.TryKindle())
        {
            RefreshMenu();

            StartCoroutine(SelectLeaveNextFrame());
        }
    }

    private IEnumerator SelectLeaveNextFrame()
    {
        // Wait until Unity finishes processing the button click.
        yield return null;

        if (EventSystem.current != null &&
            leaveButton != null &&
            leaveButton.gameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(null);

            EventSystem.current.SetSelectedGameObject(
                leaveButton.gameObject
            );
        }
    }

    private void Leave()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.movementLocked = false;

        // Clear the current UI selection.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        activeBonfire = null;
    }
}
