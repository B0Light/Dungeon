using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class PhysicalProjectile : BaseProjectile
{
    private IObjectPool<PhysicalProjectile> _projectilePool;
    private IObjectPool<EffectPlayer> _impactParticlePool;
    private IObjectPool<EffectPlayer> _effectObjectPool;
    private Vector3 _velocity;
    private float _traveledDistance;
    private readonly int _maxDetectionCount = 50;
    
    [Header("Static Effect Settings")]
    [SerializeField] private bool isStaticEffect = false;
    [SerializeField] private float staticEffectDuration = 2f;
    
    [Header("Advanced Physics")]
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float gravityMultiplier = 1f;

    public void SetPools(IObjectPool<PhysicalProjectile> projectilePool, IObjectPool<EffectPlayer> impactParticlePool, IObjectPool<EffectPlayer> effectObjectPool)
    {
        _projectilePool = projectilePool;
        _impactParticlePool = impactParticlePool;
        _effectObjectPool = effectObjectPool;
    }

    public override void Fire(Vector3 position, Vector3 direction, Transform firePoint)
    {
        // 초기화
        charactersDamaged.Clear();
        _isActive = true;
        
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction);
        
        _velocity = direction * _config.projectileSpeed;
        _traveledDistance = 0f;

        // 발사 모드에 따른 처리
        if (_config.projectileSpeed <= 0f || isStaticEffect)
        {
            StartCoroutine(StaticEffectHandler());
        }
        else
        {
            StartCoroutine(ProjectileMovement());
        }
    }

    private void SpawnImpactParticle(Vector3 position)
    {
        if (_impactParticlePool != null && _config.impactParticlePrefab != null)
        {
            EffectPlayer impactParticle = _impactParticlePool.Get();
            if (impactParticle != null)
            {
                impactParticle.transform.position = position;
                impactParticle.transform.rotation = transform.rotation;
                
                // 파티클 시스템 자동 재생 및 자동 반환 설정
                var particleSystem = impactParticle.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                    StartCoroutine(ReturnImpactParticleAfterPlay(impactParticle, particleSystem));
                }
            }
        }
    }

    private IEnumerator StaticEffectHandler()
    {
        // 즉시 범위 공격 실행
        StartCoroutine(PerformProgressiveAreaDamage());
        
        // 이펙트 지속 시간 계산
        float effectDuration = CalculateEffectDuration();
        
        // 이펙트 지속 시간만큼 대기
        yield return new WaitForSeconds(effectDuration);
        
        ReturnToPool();
    }

    private float CalculateEffectDuration()
    {
        return staticEffectDuration;
    }
    
    private IEnumerator PerformProgressiveAreaDamage()
    {
        float baseRadius = _config.areaOfEffect > 0 ? _config.areaOfEffect : 2f;
        float delayBetweenStages = 0.1f;
    
        // 단계별 범위 설정
        float[] stageRadii = {
            baseRadius * 0.3f,
            baseRadius * 0.6f,
            baseRadius
        };
    
        HashSet<Collider> alreadyHitTargets = new HashSet<Collider>();
        Collider[] colliderBuffer = new Collider[_maxDetectionCount];
    
        for (int stage = 0; stage < stageRadii.Length; stage++)
        {
            yield return StartCoroutine(ProcessStageAreaDamage(stageRadii[stage], colliderBuffer, alreadyHitTargets));
            
            if (stage < stageRadii.Length - 1)
            {
                yield return new WaitForSeconds(delayBetweenStages);
            }
        }
    }

    private IEnumerator ProcessStageAreaDamage(float radius, Collider[] buffer, HashSet<Collider> alreadyHit)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            radius,
            buffer,
            _config.collisionMask);
    
        for (int i = 0; i < hitCount; i++)
        {
            var target = buffer[i];
            if (target && !alreadyHit.Contains(target))
            {
                ProcessStaticHit(target, target.ClosestPoint(transform.position));
                alreadyHit.Add(target);
                
                // 약간의 딜레이로 자연스러운 연쇄 효과
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        // 버퍼 정리
        Array.Clear(buffer, 0, buffer.Length);
    }
    
    private void ProcessStaticHit(Collider hitCollider, Vector3 hitPoint)
    {
        CharacterManager targetCharacter = GetTargetCharacter(hitCollider);

        if (targetCharacter != null && !targetCharacter.isDead.Value)
        {
            if (ShouldDamageTarget(targetCharacter))
            {
                ProcessCharacterHit(targetCharacter, hitPoint);
            }
        }
        else
        {
            // 환경 충돌 시 파티클 생성
            SpawnImpactParticle(hitPoint);
            PlayEffect(hitPoint);
        }
    }

    private IEnumerator ProjectileMovement()
    {
        while (_isActive && _traveledDistance < _config.maxRange)
        {
            float deltaTime = Time.deltaTime;
            
            // 중력 적용
            if (useGravity)
            {
                _velocity += Vector3.down * (Physics.gravity.magnitude * gravityMultiplier * deltaTime);
            }
            
            Vector3 movement = _velocity * deltaTime;
            
            // 레이캐스트로 충돌 검사
            if (Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit hit, 
                               movement.magnitude, _config.collisionMask))
            {
                // 충돌 지점으로 이동
                transform.position = hit.point;

                // 충돌 처리
                HandleCollision(hit);
                
            }
            else
            {
                // 정상 이동
                transform.position += movement;
                _traveledDistance += movement.magnitude;
                
                // 방향 업데이트 (중력 적용 시)
                if (useGravity && _velocity.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(_velocity.normalized);
                }
            }

            yield return null;
        }

        ReturnToPool();
    }

    protected override void OnHitEnvironment(RaycastHit hit)
    {
        base.OnHitEnvironment(hit);
        PlayEffect(hit.point);
        StartCoroutine(PerformExplosionDamage(hit.point));
    }

    private IEnumerator ReturnImpactParticleAfterPlay(EffectPlayer impactParticle, ParticleSystem particleSystem)
    {
        // 파티클 시스템이 완전히 끝날 때까지 대기
        while (particleSystem.isPlaying)
        {
            yield return null;
        }
        
        // 추가적으로 모든 파티클이 사라질 때까지 대기
        yield return new WaitForSeconds(particleSystem.main.startLifetime.constantMax);
        
        // 풀로 반환
        _impactParticlePool?.Release(impactParticle);
    }

    private CharacterManager GetTargetCharacter(Collider hitCollider)
    {
        CharacterManager target = hitCollider.GetComponent<CharacterManager>();
        if (target == null)
        {
            target = hitCollider.GetComponentInParent<CharacterManager>();
        }
        return target;
    }

    private void ProcessCharacterHit(CharacterManager targetCharacter, Vector3 hitPoint)
    {
        contactPoint = hitPoint;
        SetBlockingDotValues(targetCharacter);
        
        if (CheckForParried(targetCharacter)) return;
        
        bool isBlocked = CheckForBlock(targetCharacter);
        DamageTarget(targetCharacter, isBlocked);
        
        if (_config.applyStatusEffect && _config.statusEffectType != StatusEffectType.None)
        {
            ApplyStatusEffect(targetCharacter);
        }
        
        // 충돌 파티클 생성
        SpawnImpactParticle(hitPoint);
        
        // 기존 히트 이펙트도 재생 (필요한 경우)
        PlayEffect(hitPoint);
    }

    private void PlayEffect(Vector3 hitPoint)
    {
        if (_effectObjectPool != null)
        {
            EffectPlayer effectPlayer = _effectObjectPool.Get();
            if (effectPlayer != null)
            {
                effectPlayer.transform.position = hitPoint;
                effectPlayer.transform.rotation = quaternion.identity;
                effectPlayer.SetResource();
                effectPlayer.PlayAllParticles();
                
                // 이펙트 재생 후 자동으로 풀에 반환
                StartCoroutine(ReturnEffectAfterPlay(effectPlayer));
            }
        }
    }
    
    private IEnumerator ReturnEffectAfterPlay(EffectPlayer effectPlayer)
    {
        // 이펙트가 모두 재생될 때까지 기다림
        yield return new WaitForSeconds(2f); // 기본 2초, 필요시 조정 가능
        
        // 풀로 반환
        _effectObjectPool?.Release(effectPlayer);
    }
    
    /// <summary>
    /// 폭발 데미지를 즉시 처리
    /// </summary>
    private IEnumerator PerformExplosionDamage(Vector3 explosionCenter)
    {
        float radius = _config.areaOfEffect > 0 ? _config.areaOfEffect : 3f; // 기본 3m 반지름
        // 범위 내 모든 콜라이더 찾기
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, radius, _config.collisionMask);
        
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider != null)
            {
                // 거리 기반 데미지 감소 계산
                float distance = Vector3.Distance(explosionCenter, hitCollider.transform.position);
                float damageMultiplier = Mathf.Clamp01(1f - (distance / radius)); // 거리에 따라 데미지 감소
                
                // 캐릭터인지 확인
                CharacterManager targetCharacter = GetTargetCharacter(hitCollider);
                if (targetCharacter != null && ShouldDamageTarget(targetCharacter))
                {
                    // 임시 데미지 저장
                    float originalPhysicalDamage = physicalDamage;
                    float originalMagicalDamage = magicalDamage;
                    
                    // 거리에 따른 데미지 적용
                    physicalDamage *= damageMultiplier;
                    magicalDamage *= damageMultiplier;
                    // 폭발 지점에서 타겟으로의 가짜 히트 생성
                    contactPoint = hitCollider.ClosestPoint(explosionCenter);
                    
                    // 기존 데미지 시스템 사용
                    SetBlockingDotValues(targetCharacter);
                    bool isBlocked = CheckForBlock(targetCharacter);
                    DamageTarget(targetCharacter, isBlocked);
                    
                    // 원래 데미지 복구
                    physicalDamage = originalPhysicalDamage;
                    magicalDamage = originalMagicalDamage;
                    
                    yield return new WaitForSeconds(0.01f); // 약간의 딜레이
                }
            }
        }
    }

    public override void ReturnToPool()
    {
        _isActive = false;
        StopAllCoroutines();

        // 풀로 반환
        _projectilePool?.Release(this);
    }
}