using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject levelUpPanel;

    [Header("Player Information")]
    public TMP_Text playerLevelText;
    public TMP_Text availableXPText;

    [Header("Attribute Buttons")]
    public Button vigorButton;
    public Button mindButton;
    public Button strengthButton;
    public Button enduranceButton;

    [Header("Attribute Level Text")]
    public TMP_Text vigorLevelText;
    public TMP_Text mindLevelText;
    public TMP_Text strengthLevelText;
    public TMP_Text enduranceLevelText;

    [Header("Upgrade Preview")]
    public TMP_Text selectedAttributeText;
    public TMP_Text attributeDescriptionText;
    public TMP_Text requiredXPText;
    public TMP_Text updateText;

    [Header("Footer Buttons")]
    public Button confirmButton;
    public Button cancelButton;
    public Button closePageButton;

    [Header("Colours")]
    public Color normalLevelColour = Color.white;
    public Color pendingLevelColour = Color.green;

    private PlayerAttributes playerAttributes;
    private PlayerXP playerXP;
    private PlayerAudio playerAudio;
    private BonfireUI bonfireUI;

    private AttributeType selectedAttribute;
    private bool hasSelectedAttribute;
    private string feedbackMessage = "";

    private void Start()
    {
        // Find the player.
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerAttributes = player.GetComponent<PlayerAttributes>();
            playerXP = player.GetComponent<PlayerXP>();
            playerAudio = player.GetComponent<PlayerAudio>();
        }

        bonfireUI = FindFirstObjectByType<BonfireUI>();

        // Connect button events.
        vigorButton.onClick.AddListener(
            () => AllocateUpgrade(AttributeType.Health));

        mindButton.onClick.AddListener(
            () => AllocateUpgrade(AttributeType.Magic));

        strengthButton.onClick.AddListener(
            () => AllocateUpgrade(AttributeType.Strength));

        enduranceButton.onClick.AddListener(
            () => AllocateUpgrade(AttributeType.Stamina));

        confirmButton.onClick.AddListener(ConfirmUpgrades);
        cancelButton.onClick.AddListener(CancelUpgrades);
        closePageButton.onClick.AddListener(ClosePage);

        // Hide the panel initially.
        levelUpPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateSelectedAttribute();
    }

    // --------------------------------------------------
    // OPEN AND CLOSE
    // --------------------------------------------------

    public void Open()
    {
        if (playerAttributes == null || levelUpPanel == null)
            return;

        feedbackMessage = "";

        levelUpPanel.SetActive(true);

        RefreshUI();

        // Select Vigor automatically.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(
                vigorButton.gameObject
            );
        }

        selectedAttribute = AttributeType.Health;
        hasSelectedAttribute = true;

        RefreshAttributeDescription();
    }

    private void ClosePage()
    {
        playerAttributes.CancelUpgrades();
        feedbackMessage = "";

        if (playerAudio != null)
            playerAudio.PlayUIBack();

        // Close the Level Up panel.
        levelUpPanel.SetActive(false);
        hasSelectedAttribute = false;

        // Select Leave on the Bonfire menu.
        StartCoroutine(SelectBonfireLeaveButton());
    }

    private IEnumerator SelectBonfireLeaveButton()
    {
        // Wait until Unity has finished closing the Level Up panel.
        yield return null;

        if (bonfireUI != null &&
            bonfireUI.leaveButton != null &&
            EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            EventSystem.current.SetSelectedGameObject(
                bonfireUI.leaveButton.gameObject
            );
        }
    }

    // --------------------------------------------------
    // ATTRIBUTE SELECTION
    // --------------------------------------------------

    private void UpdateSelectedAttribute()
    {
        if (!levelUpPanel.activeInHierarchy ||
            EventSystem.current == null)
            return;

        GameObject selectedObject =
            EventSystem.current.currentSelectedGameObject;

        AttributeType newAttribute;

        if (selectedObject == vigorButton.gameObject)
            newAttribute = AttributeType.Health;

        else if (selectedObject == mindButton.gameObject)
            newAttribute = AttributeType.Magic;

        else if (selectedObject == strengthButton.gameObject)
            newAttribute = AttributeType.Strength;

        else if (selectedObject == enduranceButton.gameObject)
            newAttribute = AttributeType.Stamina;

        else
            return;

        if (!hasSelectedAttribute ||
            selectedAttribute != newAttribute)
        {
            selectedAttribute = newAttribute;
            hasSelectedAttribute = true;
            feedbackMessage = "";

            // Play selection sound.
            if (playerAudio != null)
                playerAudio.PlayUISelect();

            RefreshAttributeDescription();
            RefreshRequiredXP();
        }
    }

    private void RefreshAttributeDescription()
    {
        selectedAttributeText.text = selectedAttribute.ToString();

        switch (selectedAttribute)
        {
            case AttributeType.Health:
                attributeDescriptionText.text =
                    "Increases your maximum health.";
                break;

            case AttributeType.Magic:
                attributeDescriptionText.text =
                    "Increases your maximum magic points.";
                break;

            case AttributeType.Strength:
                attributeDescriptionText.text =
                    "Increases the damage dealt by physical attacks and magic projectiles.";
                break;

            case AttributeType.Stamina:
                attributeDescriptionText.text =
                    "Increases your maximum stamina.";
                break;
        }

        if (playerAttributes.IsAttributeMaxedOut(selectedAttribute))
        {
            attributeDescriptionText.text +=
                "\n\nThis attribute has reached its maximum level.";
        }

        updateText.text = feedbackMessage;
    }

    // --------------------------------------------------
    // ALLOCATING UPGRADES
    // --------------------------------------------------

    private void AllocateUpgrade(AttributeType attribute)
    {
        feedbackMessage = "";

        if (playerAttributes.QueueUpgrade(attribute))
        {
            feedbackMessage = "Upgrade allocated!";

            if (playerAudio != null)
                playerAudio.PlayUpgradeAllocate();
        }
        else if (playerAttributes.IsAttributeMaxedOut(attribute))
        {
            feedbackMessage =
                "This attribute has reached its maximum level.";

            if (playerAudio != null)
                playerAudio.PlayUpgradeError();
        }
        else
        {
            feedbackMessage =
                "You don't have enough XP to allocate this upgrade.";

            if (playerAudio != null)
                playerAudio.PlayUpgradeError();
        }

        selectedAttribute = attribute;
        hasSelectedAttribute = true;

        RefreshUI();
        RefreshAttributeDescription();
    }

    // --------------------------------------------------
    // CONFIRMING AND CANCELLING
    // --------------------------------------------------

    private void ConfirmUpgrades()
    {
        if (playerAttributes.GetPendingUpgradeCount() == 0)
        {
            feedbackMessage = "You haven't allocated any upgrades.";

            if (playerAudio != null)
                playerAudio.PlayUpgradeError();

            RefreshAttributeDescription();
            return;
        }

        if (playerAttributes.GetAvailableXP() < 0)
        {
            feedbackMessage =
                "You don't have enough XP to purchase these upgrades.";

            if (playerAudio != null)
                playerAudio.PlayUpgradeError();

            RefreshAttributeDescription();
            return;
        }

        if (playerAttributes.ConfirmUpgrades())
        {
            feedbackMessage = "Your upgrades have been purchased!";

            if (playerAudio != null)
                playerAudio.PlayUpgradeConfirm();
        }
        else
        {
            feedbackMessage =
                "You don't have enough XP to purchase these upgrades.";

            if (playerAudio != null)
                playerAudio.PlayUpgradeError();
        }

        RefreshUI();
        RefreshAttributeDescription();
    }

    private void CancelUpgrades()
    {
        playerAttributes.CancelUpgrades();

        if (playerAudio != null)
            playerAudio.PlayUIBack();

        feedbackMessage = "Pending upgrades cancelled.";

        RefreshUI();
        RefreshAttributeDescription();
    }

    // --------------------------------------------------
    // UI REFRESH
    // --------------------------------------------------

    private void RefreshUI()
    {
        if (playerAttributes == null || playerXP == null)
            return;

        // Player level includes pending upgrades in its preview.
        playerLevelText.text =
            "Level " +
            (playerAttributes.overallLevel +
             playerAttributes.GetPendingUpgradeCount());

        // Display XP after pending upgrade costs.
        availableXPText.text =
            playerAttributes.GetAvailableXP() + " XP";

        // Update attribute levels.
        RefreshAttributeLevel(
            AttributeType.Health,
            vigorLevelText
        );

        RefreshAttributeLevel(
            AttributeType.Magic,
            mindLevelText
        );

        RefreshAttributeLevel(
            AttributeType.Strength,
            strengthLevelText
        );

        RefreshAttributeLevel(
            AttributeType.Stamina,
            enduranceLevelText
        );

        RefreshRequiredXP();
    }

    private void RefreshAttributeLevel(
        AttributeType attribute,
        TMP_Text levelText)
    {
        int currentLevel =
            playerAttributes.GetCurrentAttributeLevel(attribute);

        int displayedLevel =
            playerAttributes.GetDisplayedAttributeLevel(attribute);

        levelText.text = displayedLevel.ToString();

        // Green means there are unconfirmed upgrades.
        levelText.color =
            displayedLevel > currentLevel
                ? pendingLevelColour
                : normalLevelColour;
    }

    private void RefreshRequiredXP()
    {
        if (playerAttributes.IsAttributeMaxedOut(selectedAttribute))
        {
            requiredXPText.text = "MAX LEVEL";
            return;
        }

        requiredXPText.text =
            playerAttributes.GetNextUpgradeCost() + " XP";
    }
}