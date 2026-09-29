using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Sword Attacks")]
    public AudioClip lightAttack1SFX;
    public AudioClip lightAttack2SFX;
    public AudioClip heavyAttackSFX;

    [Header("Combat")]
    public AudioClip swordHitSFX;
    public AudioClip blockHitSFX;

    [Header("Movement")]
    public AudioClip dodgeSFX;
    public AudioClip jumpSFX;
    public AudioClip landSFX;

    [Header("Magic")]
    public AudioClip spellCastSFX;
    public AudioClip heavySpellSFX;

    [Header("Equipment")]
    public AudioClip weaponSwapSFX;

    public void PlayLightAttack1()
    {
        PlaySound(lightAttack1SFX);
    }

    public void PlayLightAttack2()
    {
        PlaySound(lightAttack2SFX);
    }

    public void PlayHeavyAttack()
    {
        PlaySound(heavyAttackSFX);
    }

    public void PlaySwordHit()
    {
        PlaySound(swordHitSFX);
    }

    public void PlayBlockHit()
    {
        PlaySound(blockHitSFX);
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

    public void PlaySpellCast()
    {
        PlaySound(spellCastSFX);
    }

    public void PlayHeavySpell()
    {
        PlaySound(heavySpellSFX);
    }

    public void PlayWeaponSwap()
    {
        PlaySound(weaponSwapSFX);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}