using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerSoundFXManager : CharacterSoundFXManager
{
    private PlayerManager _player;
    
    [Header("IK Footstep Settings")]
    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;
    private readonly float _footstepCooldown = 0.15f;
    private readonly float _minimumVelocity = 2.0f;
    
    // .12f : sprint 
    private readonly float _groundCheckDistance = 0.08f;
    private readonly float _footDownThreshold = 0.01f; // 발이 내려오는 속도 임계값
    
    private Vector3 _lastLeftFootPos;
    private Vector3 _lastRightFootPos;
    private float _lastFootstepTime;
    
    [Header("Sound Emitter")] 
    [SerializeField] private LayerMask reactLayer; // 적을 감지할 레이어 설정

    protected override void Awake()
    {
        base.Awake();

        _player = characterManager as PlayerManager;
    }
    
    private void Start()
    {
        if (leftFoot != null) _lastLeftFootPos = leftFoot.position;
        if (rightFoot != null) _lastRightFootPos = rightFoot.position;
    }
    
    private void LateUpdate()
    {
        // 애니메이션 적용 후 발 위치 체크
        CheckFootMovement();
    }
    
    private void CheckFootMovement()
    {
        if (_player == null)
        {
            Debug.LogWarning("NO Character Manager");
            _player = characterManager as PlayerManager;
            return;
        }
        
        if(_player.characterVariableManager.CLVM.isCrouching) return;
        
        // 캐릭터가 충분히 빠르게 움직이는지 체크
        bool isMovingFast = _player.characterVariableManager.CLVM.velocity.magnitude > _minimumVelocity;
        if (!isMovingFast)
        {
            //Debug.LogWarning("SLOW : " + Mathf.Round(_player.characterVariableManager.CLVM.velocity.magnitude* 100f) / 100f);
            return;
        }
        
        // 쿨다운 체크
        if (Time.time - _lastFootstepTime < _footstepCooldown)
        {
            //Debug.LogWarning("CoolDown");
            return;
        }
        
        //Debug.LogWarning("cur speed : " + Mathf.Round(_player.characterVariableManager.CLVM.velocity.magnitude* 100f) / 100f);
        // 왼발 체크
        if (leftFoot != null)
        {
            //Debug.LogWarning("Left");
            CheckFootForStep(leftFoot, ref _lastLeftFootPos, "Left");
        }
        
        // 오른발 체크
        if (rightFoot != null)
        {
            //Debug.LogWarning("Right");
            CheckFootForStep(rightFoot, ref _lastRightFootPos, "Right");
        }
    }
    
    private void CheckFootForStep(Transform foot, ref Vector3 lastFootPos, string footName)
    {
        Vector3 currentFootPos = foot.position;
        Vector3 footMovement = currentFootPos - lastFootPos;
        
        // 발이 아래로 움직이고 있는지 체크 (착지 감지)
        bool isFootMovingDown = footMovement.y < -_footDownThreshold * Time.deltaTime;
        //Debug.Log("isFootMoving : " + isFootMovingDown);
        // 발이 땅에 닿았는지 체크
        bool isFootGrounded = Physics.Raycast(currentFootPos, Vector3.down, 
            _groundCheckDistance, WorldUtilityManager.Instance.GetEnvLayer());
        //Debug.Log("isFootGrounded : " + isFootGrounded);
        // 이전 프레임에서는 땅에 닿지 않았고, 현재는 땅에 닿았으며, 발이 아래로 움직이고 있다면
        
        bool wasGroundedLastFrame = Physics.Raycast(lastFootPos, Vector3.down, 
            _groundCheckDistance, WorldUtilityManager.Instance.GetEnvLayer());
        
        //Debug.Log("(false) wasGroundedLastFrame : " + wasGroundedLastFrame);
        if (isFootGrounded && !wasGroundedLastFrame && isFootMovingDown)
        {
            PlayFootStepSoundFX();
            _lastFootstepTime = Time.time;
            //Debug.Log($"{footName} foot step detected");
        }
        
        lastFootPos = currentFootPos;
    }

    public override void PlaySoundFX(AudioClip soundFX, float volume = 1)
    {
        base.PlaySoundFX(soundFX, volume);
        EmitSound(volume);
    }
    
    private Collider[] colliderBuffer = new Collider[32]; // 클래스 멤버 변수로 선언

    private void EmitSound(float volume)
    {
        //Debug.Log("Sound EMIT");
        // 소리가 발생할 때 물리적인 충돌을 검사하여 적을 감지
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, volume, colliderBuffer, reactLayer);
    
        for (int i = 0; i < hitCount; i++)
        {
            CharacterManager reactor = colliderBuffer[i].GetComponent<CharacterManager>();
            if (reactor != null)
            {
                reactor.characterCombatManager.ReactToSound(characterManager);
            }
        }
    }

    protected override void PlayFootStepSoundFX(float volume = 1f)
    {
        volume = characterManager.characterVariableManager.CLVM.velocity.magnitude;
        base.PlayFootStepSoundFX(volume);
    }

    public override void PlayBlockSoundFX()
    {
        PlaySoundFX(WorldSoundFXManager.Instance.ChooseBlockSfx());
    }
}

