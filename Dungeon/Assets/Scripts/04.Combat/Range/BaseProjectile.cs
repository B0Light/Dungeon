using UnityEngine;

public abstract class BaseProjectile : DamageLogic, IProjectile
{
    protected ProjectileConfiguration _config;
    protected bool _isActive;

    public virtual void Initialize(ProjectileConfiguration config)
    {
        _config = config;
        _isActive = false;
        charactersDamaged.Clear();
        // DamageLogic의 데미지 값들을 config에서 설정
        physicalDamage = config.damage * config.physicalDamageRatio;
        magicalDamage = config.damage * config.magicalDamageRatio;
        poiseDamage = config.poiseDamage;
    }

    public void SetOwner(CharacterManager owner)
    {
        ownerCharacter = owner;
    }

    public abstract void Fire(Vector3 position, Vector3 direction, Transform firePoint);
    public abstract void ReturnToPool();

    protected void HandleCollision(RaycastHit hit)
    {
        // CharacterManager 컴포넌트 찾기
        CharacterManager targetCharacter = hit.collider.GetComponent<CharacterManager>();
        if (targetCharacter == null)
        {
            // 부모에서 찾아보기 (히트박스가 자식 오브젝트인 경우)
            targetCharacter = hit.collider.GetComponentInParent<CharacterManager>();
        }

        if (targetCharacter != null && !targetCharacter.isDead.Value)
        {
            // 아군인지 확인 (캐릭터 그룹 체크)
            if (ShouldDamageTarget(targetCharacter))
            {
                ApplyDamageToTarget(targetCharacter, hit);
            }
        }
        else
        {
            // CharacterManager가 없는 오브젝트에 충돌 (벽, 장애물 등)
            OnHitEnvironment(hit);
        }
    }

    protected virtual bool ShouldDamageTarget(CharacterManager target)
    {
        // 캐릭터 그룹이 같으면 데미지를 주지 않음 (아군 공격 방지)
        if (ownerCharacter != null && ownerCharacter.characterGroup == target.characterGroup)
            return false;
            
        // 이미 데미지를 준 대상이면 데미지를 주지 않음 (중복 데미지 방지)
        if (charactersDamaged.Contains(target))
            return false;
            
        return true;
    }

    protected virtual void ApplyDamageToTarget(CharacterManager target, RaycastHit hit)
    {
        // contactPoint 설정
        contactPoint = hit.point;
        
        // DamageLogic의 기존 시스템 사용
        SetBlockingDotValues(target);
        
        // 패리 체크
        if (CheckForParried(target))
        {
            return; // 패리되었으면 데미지 처리 안함
        }
        
        // 블록 체크 후 데미지 적용
        bool isBlocked = CheckForBlock(target);
        DamageTarget(target, isBlocked);
        
        // 상태 효과 적용 (있는 경우)
        if (_config.applyStatusEffect && _config.statusEffectType != StatusEffectType.None)
        {
            ApplyStatusEffect(target);
        }
    }

    protected virtual void ApplyStatusEffect(CharacterManager target)
    {
        // 여기서 상태 효과 적용
        // 실제 구현에서는 target의 StatusEffectManager 등을 통해 처리
        switch (_config.statusEffectType)
        {
            case StatusEffectType.Poison:
                Debug.Log($"Applied poison effect to {target.name} for {_config.statusEffectDuration}s");
                break;
            case StatusEffectType.Burn:
                Debug.Log($"Applied burn effect to {target.name} for {_config.statusEffectDuration}s");
                break;
            case StatusEffectType.Freeze:
                Debug.Log($"Applied freeze effect to {target.name} for {_config.statusEffectDuration}s");
                break;
            case StatusEffectType.Stun:
                Debug.Log($"Applied stun effect to {target.name} for {_config.statusEffectDuration}s");
                break;
            case StatusEffectType.Slow:
                Debug.Log($"Applied slow effect to {target.name} for {_config.statusEffectDuration}s");
                break;
            case StatusEffectType.Bleeding:
                Debug.Log($"Applied bleeding effect to {target.name} for {_config.statusEffectDuration}s");
                break;
        }
    }

    protected virtual void OnHitEnvironment(RaycastHit hit)
    {
        // 환경 오브젝트에 충돌했을 때의 처리
        Debug.Log($"Projectile hit environment: {hit.collider.name} at {hit.point}");
    }

    // DamageLogic의 ModifyDamageEffect를 오버라이드하여 추가 효과 적용 가능
    protected override void ModifyDamageEffect(TakeDamageEffect damageEffect)
    {
        base.ModifyDamageEffect(damageEffect);
        
        // 투사체별 추가 데미지 효과 적용
        if (_config.damageModifier != 1f)
        {
            damageEffect.ApplyAttackDamageModifiers(_config.damageModifier);
        }
    }
}