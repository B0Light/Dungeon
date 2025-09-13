using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EffectPlayer))]
public class AuraVFXDamageCollider : DamageCollider
{
    private EffectPlayer _effectPlayer;

    protected override void Start()
    {
        base.Start();
        _effectPlayer = GetComponent<EffectPlayer>();
        _effectPlayer.SetResource();
    }

    public override void EnableDamageCollider()
    {
        base.EnableDamageCollider();
        _effectPlayer.PlayAllParticles();
    }

    public void EnableDamageColliderAfterDelay(float delayTime, float returnTime)
    {
        // 먼저 이펙트를 실행
        _effectPlayer.PlayAllParticles();
    
        // 코루틴으로 지연 실행
        StartCoroutine(EnableDamageColliderCoroutine(delayTime, returnTime));
    }

    private IEnumerator EnableDamageColliderCoroutine(float delayTime, float returnTime)
    {
        yield return new WaitForSeconds(delayTime);
        base.EnableDamageCollider();
        yield return new WaitForSeconds(returnTime-delayTime);
        base.DisableDamageCollider();
    }
    
}