using UnityEngine;
using System.Collections;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;
    public AudioSource enemyHitSource;

    [Header("Sword Attacks")]
    public AudioClip lightAttack1SFX;
    public AudioClip lightAttack2SFX;
    public AudioClip heavyAttackSFX;
   

    [Header("Combat")]
    public AudioClip swordHitSFX;
    public AudioClip blockHitSFX;
    public AudioClip enemyHitSFX;

    [Header("Death")]
    public AudioClip deathSFX;

    [Header("Movement")]
    public AudioClip dodgeSFX;
    public AudioClip jumpSFX;
    public AudioClip landSFX;

    [Header("Footsteps")]
    public AudioSource footstepSource;
    public AudioClip footstep1SFX;
    public AudioClip footstep2SFX;

    [Header("Items")]
    public AudioClip itemPickupSFX;

    [Range(0.5f, 1.5f)]
    public float minFootstepPitch = 0.9f;

    [Range(0.5f, 1.5f)]
    public float maxFootstepPitch = 1.1f;

    private bool useFirstFootstep = true;

    [Header("Magic")]
    public AudioClip spellCastSFX;
    public AudioClip heavySpellSFX;
    public AudioClip spellImpactSFX;

    [Header("Equipment")]
    public AudioClip weaponSwapSFX;

    [Header("Items")]
    public AudioClip potionDrinkSFX;

    [Header("Bonfire")]
    public AudioClip bonfireLightSFX;
    public AudioClip bonfireRestSFX;

    [Header("XP Recovery")]
    public AudioClip xpRecoverySFX;

    [Header("Level Up UI")]
    public AudioClip uiSelectSFX;
    public AudioClip upgradeAllocateSFX;
    public AudioClip upgradeErrorSFX;
    public AudioClip upgradeConfirmSFX;
    public AudioClip uiBackSFX;

    public void PlayBonfireLight()
    {
        PlaySound(bonfireLightSFX);
    }

    public void PlayBonfireRest()
    {
        PlaySound(bonfireRestSFX);
    }

    public void PlayXPRecovery()
    {
        PlaySound(xpRecoverySFX);
    }

    public void PlayLightAttack1()
    {
        PlaySound(lightAttack1SFX);
    }

    public void PlayLightAttack2()
    {
        PlaySound(lightAttack2SFX);
    }


    public void PlayUISelect()
    {
        PlaySound(uiSelectSFX);
    }

    public void PlayUpgradeAllocate()
    {
        PlaySound(upgradeAllocateSFX);
    }

    public void PlayUpgradeError()
    {
        PlaySound(upgradeErrorSFX);
    }

    public void PlayUpgradeConfirm()
    {
        PlaySound(upgradeConfirmSFX);
    }

    public void PlayUIBack()
    {
        PlaySound(uiBackSFX);
    }


    [Header("Heavy Attack Audio")]
    public AudioSource heavyAttackAudioSource;
    [Range(0.1f, 1f)]
    public float heavyAttackPitch = 0.4f;

    public void PlayHeavyAttack()
    {
        if (heavyAttackSFX == null || heavyAttackAudioSource == null)
            return;

        heavyAttackAudioSource.pitch = heavyAttackPitch;
        heavyAttackAudioSource.PlayOneShot(heavyAttackSFX);
    }

    public void PlaySwordHit()
    {
        PlaySound(swordHitSFX);
    }

    public void PlayEnemyHit()
    {
        enemyHitSource.PlayOneShot(enemyHitSFX);
    }

    public void PlayBlockHit()
    {
        PlaySound(blockHitSFX);
    }

    public void PlayDeath()
    {
        PlaySound(deathSFX);
    }

    public void PlayDodge()
    {
        PlaySound(dodgeSFX);
    }

    public void PlayJump()
    {
        PlaySound(jumpSFX);
    }

    public void PlayLand()
    {
        PlaySound(landSFX);
    }
    public void PlayFootstep()
    {
        AudioClip clip = useFirstFootstep
            ? footstep1SFX
            : footstep2SFX;

        useFirstFootstep = !useFirstFootstep;

        if (clip == null || footstepSource == null)
            return;

        footstepSource.pitch = Random.Range(
            minFootstepPitch,
            maxFootstepPitch
        );

        footstepSource.PlayOneShot(clip);

        footstepSource.pitch = 1f;
    }

    public void PlayPotionDrink()
    {
        PlaySound(potionDrinkSFX);
    }


    public void PlaySpellCast()
    {
        PlaySound(spellCastSFX);
    }

    public void PlayHeavySpell()
    {
        PlaySound(heavySpellSFX);
    }

    public void PlaySpellImpact(Vector3 position)
    {
        if (spellImpactSFX != null)
            AudioSource.PlayClipAtPoint(spellImpactSFX, position);
    }

    public void PlayWeaponSwap()
    {
        PlaySound(weaponSwapSFX);
    }

    public void PlayItemPickup()
    {
        PlaySound(itemPickupSFX);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}