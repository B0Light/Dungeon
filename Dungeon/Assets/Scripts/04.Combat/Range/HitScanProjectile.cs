using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class HitscanProjectile : BaseProjectile
{
    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private ParticleSystem hitParticleSystem;
    [SerializeField] private GameObject muzzleFlashEffect;
    [SerializeField] private LineRenderer tracerEffect;
    [SerializeField] private float tracerDuration = 0.05f;

    [SerializeField] private CharacterGroup _characterGroup = CharacterGroup.Team02;

    private IObjectPool<HitscanProjectile> _pool;

    protected override void Awake()
    {
        base.Awake(); // DamageLogic의 Awake 호출
        
        if (hitEffect != null && hitParticleSystem == null)
            hitParticleSystem = hitEffect.GetComponent<ParticleSystem>();
            
        if (tracerEffect != null)
            tracerEffect.enabled = false;
    }

    public override void Initialize(ProjectileConfiguration config)
    {
        base.Initialize(config);
        
        if (ownerCharacter != null)
            _characterGroup = ownerCharacter.characterGroup;
    }

    public void SetPool(IObjectPool<HitscanProjectile> pool)
    {
        _pool = pool;
    }

    public override void Fire(Vector3 position, Vector3 direction, Transform firePoint)
    {
        charactersDamaged.Clear();
        direction = direction.normalized;
        _isActive = true;
        
        // 발사 위치와 방향 설정
        transform.position = position;
        transform.forward = direction;
        
        // 머즐 플래시 효과 표시
        ShowMuzzleFlash(firePoint);
        
        // 레이캐스트로 히트 판정 - config의 값들 사용
        if (Physics.Raycast(position, direction, out RaycastHit hit, _config.maxRange, _config.collisionMask))
        {
            // 레이캐스트 히트 처리
            HandleHit(hit, position);
        }
        else
        {
            // 레이캐스트가 어떤 객체에도 맞지 않았을 때의 처리
            Debug.Log("Hit Nothing");
            Vector3 endPoint = position + (direction * _config.maxRange);
            ShowTracerEffect(position, endPoint);
        }
        
        // 일정 시간 후 오브젝트 풀로 반환
        StartCoroutine(DelayedDeactivate());
    }

    private void HandleHit(RaycastHit hit, Vector3 firePosition)
    {
        Debug.Log("Handle Hit : " + hit.collider.gameObject.name);
        Vector3 hitPoint = hit.point;
        Vector3 hitNormal = hit.normal;
        
        // 히트 이펙트 표시
        SpawnHitEffect(hitPoint, hitNormal);
        
        // 트레이서 이펙트 표시
        ShowTracerEffect(firePosition, hitPoint);
        
        // 데미지 처리 - 기존 방식 그대로
        CharacterManager damageTarget = hit.collider.GetComponentInParent<CharacterManager>();
        if (damageTarget != null && _characterGroup != damageTarget.characterGroup)
        {
            contactPoint = hit.point;
            
            SetBlockingDotValues(damageTarget);
            if (!CheckForParried(damageTarget))
            {
                DamageTarget(damageTarget, CheckForBlock(damageTarget));
            }
        }
    }

    private void ShowMuzzleFlash(Transform firePoint)
    {
        if (muzzleFlashEffect == null) return;
        
        muzzleFlashEffect.SetActive(true);
        if (firePoint != null)
        {
            muzzleFlashEffect.transform.position = firePoint.position;
            muzzleFlashEffect.transform.rotation = firePoint.rotation;
        }
        else
        {
            muzzleFlashEffect.transform.position = transform.position;
            muzzleFlashEffect.transform.rotation = transform.rotation;
        }
        
        // 머즐 플래시 파티클 시스템 재생
        ParticleSystem muzzlePS = muzzleFlashEffect.GetComponent<ParticleSystem>();
        if (muzzlePS != null)
        {
            muzzlePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzlePS.Play();
        }
    }

    private void ShowTracerEffect(Vector3 startPoint, Vector3 endPoint)
    {
        if (tracerEffect == null) return;
        
        tracerEffect.enabled = true;
        tracerEffect.SetPosition(0, startPoint);
        tracerEffect.SetPosition(1, endPoint);
        
        // 트레이서 효과는 짧은 시간만 표시
        StartCoroutine(DisableTracerAfterDuration());
    }

    private IEnumerator DisableTracerAfterDuration()
    {
        yield return new WaitForSeconds(tracerDuration);
        if (tracerEffect != null)
            tracerEffect.enabled = false;
    }

    private void SpawnHitEffect(Vector3 point, Vector3 normal)
    {
        if (hitEffect == null) return;

        hitEffect.transform.position = point;
        hitEffect.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        if (hitParticleSystem != null)
        {
            hitParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitParticleSystem.Play();
        }
    }

    protected override void ModifyDamageEffect(TakeDamageEffect damageEffect)
    {
        base.ModifyDamageEffect(damageEffect);
        // 히트스캔 특화 데미지 효과 수정이 필요하다면 여기에 추가
    }

    private IEnumerator DelayedDeactivate()
    {
        // 히트 이펙트와 트레이서 이펙트가 모두 보일 수 있도록 지연
        float delay = Mathf.Max(tracerDuration, hitParticleSystem != null ? hitParticleSystem.main.duration : 0.1f);
        yield return new WaitForSeconds(delay);
        
        ReturnToPool();
    }

    public override void ReturnToPool()
    {
        _isActive = false;
        StopAllCoroutines();

        // 모든 이펙트 종료
        if (tracerEffect != null)
            tracerEffect.enabled = false;

        if (_pool != null)
            _pool.Release(this);
        else
            gameObject.SetActive(false);
    }

    public void ForceDeactivate()
    {
        ReturnToPool();
    }
}