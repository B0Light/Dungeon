using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class GuidedProjectile : BaseProjectile
{
    [Header("유도 탄환 설정")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 10f;
    [SerializeField] private LayerMask collisionMask = -1;
    
    [Header("유도 시스템")]
    [SerializeField] private float trackingAccuracy = 1f; // 0~1: 유도 정확도
    [SerializeField] private float maxTurnRate = 360f; // 초당 최대 회전각도
    
    [Header("랜덤 경로 설정")]
    [SerializeField] private float sideAngle = 25f;
    [SerializeField] private float upAngle = 20f;
    private float _randomUpAngle;
    private float _randomSideAngle;
    
    [Header("이펙트")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject trailEffect;
    [SerializeField] private ParticleSystem muzzleFlash;
    
    // 내부 변수들
    private Transform _target;
    private Vector3 _targetOffset = Vector3.zero;
    private Vector3 _currentDirection;
    private Rigidbody _rb;
    private bool _hasHit = false;
    private Coroutine _lifeTimeCoroutine;
    private IObjectPool<GuidedProjectile> _pool;
    
    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
        }
        
        _rb.useGravity = false;
        _rb.isKinematic = true;
    }
    
    public override void Initialize(ProjectileConfiguration config)
    {
        base.Initialize(config);
        
        // config에서 설정값들 적용
        speed = config.projectileSpeed;
        lifeTime = 10f;
        collisionMask = config.collisionMask;
        
        // 랜덤 각도 초기화
        InitializeRandomAngles();
        
        // 상태 초기화
        _hasHit = false;
        _isActive = false;
    }
    
    public override void Fire(Vector3 position, Vector3 direction, Transform firePoint)
    {
        charactersDamaged.Clear();
        transform.position = position;
        _currentDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(_currentDirection);
        
        _isActive = true;
        
        // 머즐 플래시 효과
        PlayMuzzleFlash();
        
        // 트레일 효과 활성화
        if (trailEffect != null)
        {
            trailEffect.SetActive(true);
        }
        
        // 생존 시간 코루틴 시작
        if (_lifeTimeCoroutine != null)
        {
            StopCoroutine(_lifeTimeCoroutine);
        }
        _lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine());
        
        // 타겟을 플레이어로 자동 설정 (플레이어 태그로 찾기)
        SetTargetToPlayer();
    }
    
    private void Update()
    {
        if (!_isActive || _hasHit) return;
        
        MoveProjectile();
        CheckCollisions();
    }
    
    private void MoveProjectile()
    {
        if (_target != null)
        {
            // 유도 시스템
            Vector3 desiredDirection = CalculateGuidedDirection();
            _currentDirection = Vector3.Slerp(_currentDirection, desiredDirection, 
                trackingAccuracy * maxTurnRate * Time.deltaTime / 360f);
        }
        
        // 이동
        float moveDistance = speed * Time.deltaTime;
        transform.position += _currentDirection * moveDistance;
        transform.rotation = Quaternion.LookRotation(_currentDirection);
    }
    
    private Vector3 CalculateGuidedDirection()
    {
        Vector3 targetPosition = _target.position + _targetOffset + GetPredictedOffset();
        Vector3 toTarget = (targetPosition - transform.position).normalized;
        
        // 랜덤 각도 적용
        Vector3 crossDirection = Vector3.Cross(toTarget, Vector3.up);
        Quaternion randomRotation = Quaternion.Euler(0, _randomSideAngle, 0) * 
                                   Quaternion.AngleAxis(_randomUpAngle, crossDirection);
        
        return randomRotation * toTarget;
    }
    
    private Vector3 GetPredictedOffset()
    {
        // 타겟의 움직임을 예측하여 조준점 계산
        if (_target.TryGetComponent<Rigidbody>(out Rigidbody targetRb))
        {
            float timeToReach = Vector3.Distance(transform.position, _target.position) / speed;
            return targetRb.linearVelocity * timeToReach * 0.5f; // 예측 보정
        }
        return Vector3.zero;
    }
    
    private void CheckCollisions()
    {
        float checkDistance = speed * Time.deltaTime + 0.1f;
        
        if (Physics.Raycast(transform.position, _currentDirection, out RaycastHit hit, 
            checkDistance, collisionMask))
        {
            OnHit(hit);
        }
    }
    
    private void OnHit(RaycastHit hit)
    {
        if (_hasHit) return;
        
        _hasHit = true;
        _isActive = false;
        
        // 정확한 충돌 지점으로 이동
        transform.position = hit.point;
        
        // BaseProjectile의 충돌 처리 사용
        HandleCollision(hit);
        
        // 히트 이펙트 생성
        PlayHitEffect(hit);
        
        // 탄환 반환/파괴
        ReturnToPool();
    }
    
    private void PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
    
    private void PlayHitEffect(RaycastHit hit)
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, hit.point, 
                Quaternion.LookRotation(hit.normal));
            
            // 파티클 시스템 duration에 맞춰 자동 삭제
            if (effect.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                Destroy(effect, ps.main.duration);
            }
            else
            {
                Destroy(effect, 2f);
            }
        }
    }
    
    private void SetTargetToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SetTarget(player.transform, Vector3.up * 1f); // 플레이어 허리 높이 조준
        }
    }
    
    public void SetTarget(Transform targetTransform, Vector3 offset = default)
    {
        _target = targetTransform;
        _targetOffset = offset;
        
        // 약간의 랜덤 오프셋 추가 (완벽한 정확도 방지)
        _targetOffset += GetRandomOffset();
    }
    
    private Vector3 GetRandomOffset()
    {
        return new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.3f, 0.3f)
        );
    }
    
    private void InitializeRandomAngles()
    {
        _randomUpAngle = Random.Range(0, upAngle);
        _randomSideAngle = Random.Range(-sideAngle, sideAngle);
    }
    
    private IEnumerator LifeTimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        
        if (_isActive && !_hasHit)
        {
            ReturnToPool();
        }
    }
    
    public void SetPool(IObjectPool<GuidedProjectile> pool)
    {
        _pool = pool;
    }
    
    public override void ReturnToPool()
    {
        // 코루틴 정리
        if (_lifeTimeCoroutine != null)
        {
            StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = null;
        }
        
        // 상태 초기화
        _isActive = false;
        _hasHit = false;
        _target = null;
        
        // 트레일 효과 비활성화
        if (trailEffect != null)
        {
            trailEffect.SetActive(false);
        }
        
        // 풀로 반환 또는 파괴
        if (_pool != null)
        {
            _pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    protected override void OnHitEnvironment(RaycastHit hit)
    {
        base.OnHitEnvironment(hit);
        
        // 환경 충돌 시 추가 처리 (예: 벽 관통, 튕김 등)
        Debug.Log($"Guided projectile hit environment: {hit.collider.name}");
    }
    
    // 에디터에서 디버그용 기즈모
    private void OnDrawGizmosSelected()
    {
        if (_isActive && _target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.position + _targetOffset);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_target.position + _targetOffset, 0.5f);
        }
    }
}