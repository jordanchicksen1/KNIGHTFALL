
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image healthFill;
    public Image vitalityFill;
    public Image staminaFill;
    public Image mpFill;

    private PlayerAttributes playerAttributes;

    // Store the original dimensions and positions of each HUD element.
    private class BarPart
    {
        public RectTransform rectTransform;
        public Vector2 originalSize;
        public Vector2 originalPosition;
        public float originalPivotX;
    }

    private BarPart[] healthBarParts;
    private BarPart[] manaBarParts;
    private BarPart[] staminaBarParts;

    private int lastVigor;
    private int lastMind;
    private int lastEndurance;

    private void Start()
    {
        if (playerHealth == null)
            return;

        playerAttributes =
            playerHealth.GetComponent<PlayerAttributes>();

        if (playerAttributes == null)
        {
            Debug.LogError("PlayerAttributes not found!");
            return;
        }

        // Find the individual HUD components automatically.
        healthBarParts = GetHealthBarParts();
        manaBarParts = GetManaBarParts();
        staminaBarParts = GetStaminaBarParts();

        // Remember the current attribute levels.
        lastVigor = playerAttributes.vigor;
        lastMind = playerAttributes.mind;
        lastEndurance = playerAttributes.endurance;

        // Apply the initial bar sizes.
       // UpdateBarSizes();
    }

    private void Update()
    {
        if (playerHealth == null)
            return;

        // -------------------------------
        // EXISTING HEALTH FILL CALCULATIONS
        // -------------------------------

        float currentHealth = playerHealth.health;
        float maxHealth = playerHealth.maxHealth;

        healthFill.fillAmount =
            currentHealth / maxHealth;

        float currentStamina = playerHealth.stamina;
        float maxStamina = playerHealth.maxStamina;

        staminaFill.fillAmount =
            currentStamina / maxStamina;

        float currentMP = playerHealth.mp;
        float maxMP = playerHealth.maxMP;

        mpFill.fillAmount =
            currentMP / maxMP;

        vitalityFill.fillAmount =
            1f - (playerHealth.vitality /
            playerHealth.maxVitality);

        // -------------------------------
        // ATTRIBUTE LEVEL CHECK
        // -------------------------------

        if (playerAttributes == null)
            return;

        if (playerAttributes.vigor != lastVigor ||
            playerAttributes.mind != lastMind ||
            playerAttributes.endurance != lastEndurance)
        {
            lastVigor = playerAttributes.vigor;
            lastMind = playerAttributes.mind;
            lastEndurance = playerAttributes.endurance;

            UpdateBarSizes();
        }
    }

    // --------------------------------------------------
    // FIND HUD COMPONENTS
    // --------------------------------------------------

    private BarPart[] GetHealthBarParts()
    {
        RectTransform outline =
            healthFill.transform.parent as RectTransform;

        return CreateBarParts(
            outline,
            "healthBarBackground",
            healthFill,
            vitalityFill,
            "barContainer"
        );
    }

    private BarPart[] GetManaBarParts()
    {
        RectTransform outline =
            mpFill.transform.parent as RectTransform;

        return CreateBarParts(
            outline,
            "manaBarBackground",
            mpFill,
            null,
            "barContainer (1)"
        );
    }

    private BarPart[] GetStaminaBarParts()
    {
        RectTransform outline =
            staminaFill.transform.parent as RectTransform;

        return CreateBarParts(
            outline,
            "staminaBarBackground",
            staminaFill,
            null,
            "barContainer (2)"
        );
    }

    private BarPart[] CreateBarParts(
        RectTransform outline,
        string backgroundName,
        Image fill,
        Image secondaryFill,
        string containerName)
    {
        if (outline == null)
        {
            Debug.LogError("HUD bar outline could not be found!");
            return new BarPart[0];
        }

        RectTransform background =
            FindChildRect(outline, backgroundName);

        RectTransform container =
            FindChildRect(outline, containerName);

        return new BarPart[]
        {
            CreateBarPart(outline),
            CreateBarPart(background),
            CreateBarPart(fill != null ? fill.rectTransform : null),
            CreateBarPart(secondaryFill != null
                ? secondaryFill.rectTransform : null),
            CreateBarPart(container)
        };
    }

    private RectTransform FindChildRect(
        RectTransform parent,
        string childName)
    {
        Transform child = parent.Find(childName);

        if (child == null)
        {
            Debug.LogWarning(
                "Could not find HUD component: " + childName
            );

            return null;
        }

        return child as RectTransform;
    }

    // --------------------------------------------------
    // SAVE ORIGINAL BAR DIMENSIONS
    // --------------------------------------------------

    private BarPart CreateBarPart(RectTransform rect)
    {
        if (rect == null)
            return null;

        return new BarPart
        {
            rectTransform = rect,
            originalSize = rect.sizeDelta,
            originalPosition = rect.anchoredPosition,
            originalPivotX = rect.pivot.x
        };
    }

    // --------------------------------------------------
    // UPDATE BAR SIZES
    // --------------------------------------------------

    private void UpdateBarSizes()
    {
        float healthMultiplier =
            GetBarMultiplier(playerAttributes.vigor);

        float manaMultiplier =
            GetBarMultiplier(playerAttributes.mind);

        float staminaMultiplier =
            GetBarMultiplier(playerAttributes.endurance);

        ResizeBar(healthBarParts, healthMultiplier);
        ResizeBar(manaBarParts, manaMultiplier);
        ResizeBar(staminaBarParts, staminaMultiplier);
    }

    private float GetBarMultiplier(int attributeLevel)
    {
        return 1f + ((attributeLevel - 1) * 0.2f);
    }



    private void ResizeBar(
        BarPart[] barParts,
        float multiplier)
    {
        if (barParts == null)
            return;

        foreach (BarPart part in barParts)
        {
            if (part == null || part.rectTransform == null)
                continue;

            RectTransform rect = part.rectTransform;

            // Record the actual left edge before resizing.
            float originalLeftEdge = GetLeftEdgeWorldX(rect);

            // Calculate the new width.
            float newWidth =
                part.originalSize.x * multiplier;

            // Resize this component.
            rect.sizeDelta = new Vector2(
                newWidth,
                part.originalSize.y
            );

            // Find where the left edge moved after resizing.
            float newLeftEdge = GetLeftEdgeWorldX(rect);

            // Correct its world position to restore the left edge.
            float positionCorrection =
                originalLeftEdge - newLeftEdge;

            // Correct the component's original left edge.
            rect.position += new Vector3(
                positionCorrection,
                0f,
                0f
            );

            
        }
    }

    private float GetLeftEdgeWorldX(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];

        rect.GetWorldCorners(corners);

        float leftEdge = corners[0].x;

        for (int i = 1; i < corners.Length; i++)
        {
            leftEdge = Mathf.Min(leftEdge, corners[i].x);
        }

        return leftEdge;
    }


}
