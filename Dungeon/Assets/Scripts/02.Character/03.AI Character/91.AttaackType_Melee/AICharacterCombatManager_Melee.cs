using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICharacterCombatManager_Melee : AICharacterCombatManager
{
   [Header("Damage Collider")]
   [SerializeField] private DamageCollider[] leftHandDamageColliders;
   [SerializeField] private DamageCollider[] rightHandDamageColliders;

   [SerializeField] private DamageCollider hornDamageCollider;
   [Header("Damage")] 
   [SerializeField] protected int baseDamage = 25;
   [SerializeField] protected int basePoiseDamage = 25;
   [SerializeField] protected float attack01DamageModifier = 1.0f;
   [SerializeField] protected float attack02DamageModifier = 1.4f;

   /* Animation Event */
   public void SetAttack01Damage()
   {
      foreach (var leftHandDamageCollider in leftHandDamageColliders)
      {
         leftHandDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
         leftHandDamageCollider.poiseDamage = basePoiseDamage * attack01DamageModifier;
      }

      foreach (var rightHandDamageCollider in rightHandDamageColliders)
      {
         rightHandDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
         rightHandDamageCollider.poiseDamage = basePoiseDamage * attack01DamageModifier;
      }
   }
   
   public void SetAttack02Damage()
   {
      foreach (var leftHandDamageCollider in leftHandDamageColliders)
      {
         leftHandDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
      }

      foreach (var rightHandDamageCollider in rightHandDamageColliders)
      {
         rightHandDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
      }
   }
   
   public virtual void OpenLeftHandDamageCollider()
   {
      aiCharacter.characterSoundFXManager.PlayAttackGruntSoundFX();   
      
      foreach (var leftHandDamageCollider in leftHandDamageColliders)
      {
         leftHandDamageCollider.EnableDamageCollider();
      }
   }

   public virtual void OpenRightHandDamageCollider()
   {
      aiCharacter.characterSoundFXManager.PlayAttackGruntSoundFX();   
      
      foreach (var rightHandDamageCollider in rightHandDamageColliders)
      {
         rightHandDamageCollider.EnableDamageCollider();
      }
   }
   
   public void CloseLeftHandDamageCollider()
   {
      foreach (var leftHandDamageCollider in leftHandDamageColliders)
      {
         leftHandDamageCollider.DisableDamageCollider();
      }
   }
   
   public void CloseRightHandDamageCollider()
   {
      foreach (var rightHandDamageCollider in rightHandDamageColliders)
      {
         rightHandDamageCollider.DisableDamageCollider();
      }
   }

   public void OpenHornDamageCollider()
   {
      hornDamageCollider?.EnableDamageCollider();
   }
   
   public void CloseHornDamageCollider()
   {
      hornDamageCollider?.DisableDamageCollider();
   }
   
}
