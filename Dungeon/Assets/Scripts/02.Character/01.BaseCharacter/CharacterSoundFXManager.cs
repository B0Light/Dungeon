using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class CharacterSoundFXManager : MonoBehaviour
{
    protected CharacterManager characterManager;

    [Header("Damaged Grunts")] 
    [SerializeField] protected AudioClip[] damageGrunts;
    
    [Header("Attack Grunts")] 
    [SerializeField] protected AudioClip[] attackGrunts;
    
    [Header("Blocking")] 
    [SerializeField] protected AudioClip[] blockingSfx;
    
    [Header("Foot Steps")] 
    public AudioClip[] footsteps;
    
    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public virtual void PlaySoundFX(AudioClip soundFX, float volume = 1)
    {
        WorldSoundFXManager.Instance.PlaySfx(soundFX, volume);
    }
    
    public void PlayRollSoundFX()
    {
        PlaySoundFX(WorldSoundFXManager.Instance.rollSfx);
    }

    public void PlayDamageGruntSoundFX()
    {
        if(damageGrunts.Length > 0)
            PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(damageGrunts));
    }

    public virtual void PlayAttackGruntSoundFX()
    {
        if(attackGrunts.Length > 0)
            PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(attackGrunts), 1f);
    }

    protected virtual void PlayFootStepSoundFX(float volume = 1f)
    {
        if(footsteps.Length > 0)
            PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(footsteps), volume);
    }
    
    public virtual void PlayBlockSoundFX()
    {
        if(blockingSfx.Length > 0)
            PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(blockingSfx));
    }
}

