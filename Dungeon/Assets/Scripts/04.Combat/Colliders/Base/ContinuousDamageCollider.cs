using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ContinuousDamageCollider : DamageCollider
{
    [Header("Continuous Damage Settings")] 
    [SerializeField] private int damageValue = 10;
    [SerializeField] private float damageInterval = 1f; // 피해를 주는 간격 (초)
    [SerializeField] private bool startDamageOnEnter = true; // 진입 시 즉시 피해를 줄지 여부
    
    // 현재 콜라이더 내에 있는 캐릭터들을 추적
    private Dictionary<CharacterManager, Coroutine> activeDamageCoroutines = new Dictionary<CharacterManager, Coroutine>();

    protected override void Start()
    {
        foreach (var damageCollider in damageColliders)
        {
            damageCollider.enabled = true;
            damageCollider.isTrigger = true;
        }
        magicalDamage = damageValue;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();
        if (!damageTarget) return;
        
        // 같은 그룹이면 피해 없음
        if (ownerCharacter.characterGroup == damageTarget.characterGroup) return;
        
        contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        
        // 이미 피해를 받고 있는 캐릭터라면 중복 처리 방지
        if (activeDamageCoroutines.ContainsKey(damageTarget)) return;
        
        // 진입 시 즉시 피해
        if (startDamageOnEnter)
        {
            SetBlockingDotValues(damageTarget);
            if (CheckForParried(damageTarget)) return;
            
            DamageTarget(damageTarget, CheckForBlock(damageTarget));
        }
        
        // 지속 피해 코루틴 시작
        Coroutine damageCoroutine = StartCoroutine(ContinuousDamageCoroutine(damageTarget));
        activeDamageCoroutines.Add(damageTarget, damageCoroutine);
    }
    
    protected virtual void OnTriggerExit(Collider other)
    {
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();
        if (!damageTarget) return;
        
        // 지속 피해 중단
        StopContinuousDamage(damageTarget);
    }
    
    private IEnumerator ContinuousDamageCoroutine(CharacterManager target)
    {
        while (target != null && activeDamageCoroutines.ContainsKey(target))
        {
            yield return new WaitForSeconds(damageInterval);
            
            // 타겟이 여전히 유효한지 확인
            if (target == null || target.gameObject == null)
            {
                StopContinuousDamage(target);
                yield break;
            }
            
            // 피해 적용
            SetBlockingDotValues(target);
            if (CheckForParried(target)) continue;
            
            DamageTarget(target, CheckForBlock(target));
        }
    }
    
    private void StopContinuousDamage(CharacterManager target)
    {
        if (activeDamageCoroutines.ContainsKey(target))
        {
            if (activeDamageCoroutines[target] != null)
            {
                StopCoroutine(activeDamageCoroutines[target]);
            }
            activeDamageCoroutines.Remove(target);
        }
    }
    
    public override void EnableDamageCollider()
    {
        base.EnableDamageCollider();
        // 기존 지속 피해들을 모두 정리
        StopAllContinuousDamage();
    }
    
    public override void DisableDamageCollider()
    {
        base.DisableDamageCollider();
        // 모든 지속 피해 중단
        StopAllContinuousDamage();
    }
    
    private void StopAllContinuousDamage()
    {
        foreach (var kvp in activeDamageCoroutines)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        activeDamageCoroutines.Clear();
    }
    
    protected virtual void OnDestroy()
    {
        // 오브젝트가 파괴될 때 모든 코루틴 정리
        StopAllContinuousDamage();
    }
    
    protected virtual void OnDisable()
    {
        // 오브젝트가 비활성화될 때 모든 코루틴 정리
        StopAllContinuousDamage();
    }
}