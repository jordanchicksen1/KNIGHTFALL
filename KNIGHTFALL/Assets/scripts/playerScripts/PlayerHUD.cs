using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text weaponText;
    public Image weaponIcon;
    public TMP_Text spellText;
    public Image spellIcon; 
    public TMP_Text itemText;
    public Image itemIcon;
    public TMP_Text itemCountText;

    [Header("Weapon Icons")]
    public Sprite swordIcon;
    public Sprite staffIcon;

    [Header("Spell Icons")]
    public Sprite fireballIcon;
    public Sprite iceSpearIcon;
    public Sprite lightningBoltIcon;
    public Sprite noSpellIcon;

    [Header("Item Icons")]
    public Sprite healingFlaskIcon;
    public Sprite manaFlaskIcon;
    public Sprite emberIcon;

    [Header("XP UI")]
    public TMP_Text xpCountText;
    public TMP_Text xpGainText;

    [Header("XP Animation")]
    public float xpGainDisplayDuration = 1.5f;
    public float xpCountAnimationDuration = 0.4f;

    private PlayerXP playerXP;
    private int displayedXP;
    private int lastKnownXP;
    private Coroutine xpAnimationCoroutine;

    private PlayerCombat combat;
    private PlayerItems items;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            combat = player.GetComponent<PlayerCombat>();
            playerXP = player.GetComponent<PlayerXP>();
            items = player.GetComponent<PlayerItems>();
        }

        if (playerXP != null)
        {
            displayedXP = playerXP.currentXP;
            lastKnownXP = playerXP.currentXP;

            xpCountText.text = displayedXP.ToString();
        }

        xpGainText.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateXP();

        if (combat == null || items == null)
            return;

        UpdateWeapon();
        UpdateSpell();
        UpdateItem();
    }

    void UpdateWeapon()
    {
        weaponText.text = combat.currentWeapon.ToString();

        switch (combat.currentWeapon)
        {
            case WeaponType.Sword:
                weaponIcon.sprite = swordIcon;
                break;

            case WeaponType.Staff:
                weaponIcon.sprite = staffIcon;
                break;
        }
    }

    void UpdateSpell()
    {
        spellText.text = combat.currentSpell.ToString();

        switch (combat.currentSpell)
        {
            case SpellType.None:
                spellIcon.sprite = noSpellIcon;
                break;

            case SpellType.Fireball:
                spellIcon.sprite = fireballIcon;
                break;

            case SpellType.IceSpear:
                spellIcon.sprite = iceSpearIcon;
                break;

            case SpellType.LightningBolt:
                spellIcon.sprite = lightningBoltIcon;
                break;
        }
    }

    void UpdateItem()
    {
        switch (items.currentItem)
        {
            case ItemType.HealingFlask:
                itemText.text = "Healing Flask";
                itemIcon.sprite = healingFlaskIcon;
                itemCountText.text = items.currentHealingFlasks.ToString();
                break;

            case ItemType.ManaFlask:
                itemText.text = "Mana Flask";
                itemIcon.sprite = manaFlaskIcon;
                itemCountText.text = items.currentManaFlasks.ToString();
                break;

            case ItemType.Ember:
                itemText.text = "Ember";
                itemIcon.sprite = emberIcon;
                itemCountText.text = items.currentEmbers.ToString();
                break;
        }
    }


    void UpdateXP()
    {
        if (playerXP == null)
            return;

        int currentXP = playerXP.currentXP;

        if (currentXP == lastKnownXP)
            return;

        int xpGained = currentXP - lastKnownXP;
        lastKnownXP = currentXP;

        if (xpAnimationCoroutine != null)
            StopCoroutine(xpAnimationCoroutine);

        if (xpGained > 0)
        {
            xpAnimationCoroutine = StartCoroutine(
                AnimateXPGain(xpGained, currentXP)
            );
        }
        else
        {
            displayedXP = currentXP;
            xpCountText.text = displayedXP.ToString();
        }
    }

    IEnumerator AnimateXPGain(int amount, int targetXP)
    {
        // Display the XP gained notification.
        xpGainText.text = "+" + amount;

        Color gainColor = xpGainText.color;
        gainColor.a = 1f;
        xpGainText.color = gainColor;

        xpGainText.gameObject.SetActive(true);

        // Keep notification visible before fading.
        float waitTime = Mathf.Max(
            0f,
            xpGainDisplayDuration - xpCountAnimationDuration
        );

        yield return new WaitForSeconds(waitTime);

        // Animate the counter and fade the notification.
        float timer = 0f;
        int startingXP = displayedXP;

        while (timer < xpCountAnimationDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / xpCountAnimationDuration
            );

            float smoothProgress = Mathf.SmoothStep(
                0f, 1f, progress
            );

            displayedXP = Mathf.RoundToInt(
                Mathf.Lerp(startingXP, targetXP, smoothProgress)
            );

            xpCountText.text = displayedXP.ToString();

            gainColor.a = 1f - smoothProgress;
            xpGainText.color = gainColor;

            yield return null;
        }

        displayedXP = targetXP;
        xpCountText.text = displayedXP.ToString();

        xpGainText.gameObject.SetActive(false);

        xpAnimationCoroutine = null;
    }

}