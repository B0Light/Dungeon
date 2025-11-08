using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take BlockDamage")]
public class TakeBlockDamageEffect : TakeDamageEffect
{
    protected override void CalculateDamage(CharacterManager hitTarget)
    {
        physicalDamage *= (100 - hitTarget.characterStatsManager.blockingPhysicalAbsorption) / 100;
        magicalDamage *= (100 - hitTarget.characterStatsManager.blockingMagicalAbsorption) / 100;

        base.CalculateDamage(hitTarget);
    }

    // Overridden method to play VFX when blocked damage occurs
    protected override void PlayDamageVfx(CharacterManager character)
    {
        character.characterEffectsManager.PlayBlockVFX(contactPoint);
    }

    // Overridden method to play SFX when blocked damage occurs
    protected override void PlayDamageSfx(CharacterManager character)
    {
        AudioClip blockSfx = WorldSoundFXManager.Instance.ChooseBlockSfx();

        if (blockSfx != null)
            character.characterSoundFXManager.PlaySoundFX(blockSfx);

        character.characterSoundFXManager.PlayDamageGruntSoundFX();
    }

    // Overridden method to handle directional animation for block damage
    protected override void PlayDirectionalBasedDamagedAnimation(CharacterManager character)
    {
        if (character.isDead.Value || character.isGroggy.Value) return;

        int damageAnimationHash;

        if ((145 <= angleHitFrom && angleHitFrom <= 180) || (-145 >= angleHitFrom && angleHitFrom >= -180))
        {
            // front
            damageAnimationHash = character.characterAnimatorManager.blockForward;
        }
        else if (-144 <= angleHitFrom && angleHitFrom <= -45)
        {
            // left
            damageAnimationHash = character.characterAnimatorManager.blockLeft;
        }
        else if (45 <= angleHitFrom && angleHitFrom <= 144)
        {
            // right
            damageAnimationHash = character.characterAnimatorManager.blockRight;
        }
        else
        {
            return;
        }

        character.characterAnimatorManager.lastDamageAnimationPlayed = damageAnimationHash;
        character.characterAnimatorManager.PlayTargetActionAnimation(damageAnimationHash, true);
    }
}
