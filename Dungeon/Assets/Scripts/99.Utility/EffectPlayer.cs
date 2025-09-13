using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectPlayer : MonoBehaviour
{
    private readonly List<ParticleSystem> _particleSystems = new List<ParticleSystem>();

    private readonly List<SoundfxPlayer> _combatSfxList = new List<SoundfxPlayer>();

    public void SetResource()
    {
        _particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>());
        _combatSfxList.AddRange(GetComponentsInChildren<SoundfxPlayer>());
    }
    
    public void PlayAllParticles()
    {
        Debug.Log("particleSystem : " + _particleSystems.Count);
        foreach (ParticleSystem particle in _particleSystems)
        {
            if (particle != null)
                particle.Play();
        }
        
        foreach (var sfx in _combatSfxList)
        {
            if(sfx != null)
                sfx.PlaySfxWithDelay();
        }
    }
}
