using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterLocomotionVariableManager))]
public class CharacterVariableManager : MonoBehaviour
{
    protected CharacterManager character;
    
    private readonly int _isDeadHash = Animator.StringToHash("IsDead");
    private readonly int _groggyHash = Animator.StringToHash("GroggyEnd");
    private readonly int _isChargingHash = Animator.StringToHash("IsChargingAttack");
    private readonly int _isBlockingHash = Animator.StringToHash("IsBlocking");
    
    [HideInInspector] public CharacterLocomotionVariableManager CLVM;
    
    [Header("Position")]
    public Variable<Vector3> position = new Variable<Vector3>(Vector3.zero);
    public Variable<Quaternion> rotation = new Variable<Quaternion>(Quaternion.identity);

    [Header("Flags")]
    private bool _isInitVariable = false;
    public Variable<bool> isAttacking = new Variable<bool>(false);
    public Variable<bool> isCharging = new Variable<bool>(false);
    public Variable<bool> isInvulnerable = new Variable<bool>(false);
    public Variable<bool> isBlock = new Variable<bool>(false);
    public Variable<bool> isParring = new Variable<bool>(false);
    public ClampedVariable<float> groggy = new ClampedVariable<float>(100.0f);

    [Header("Combat")] 
    public Variable<float> physicalDamageMultiplier = new Variable<float>(1.0f);
    public Variable<float> magicalDamageMultiplier = new Variable<float>(1.0f);
    private readonly float _groggyTime = 1;
    
    [Header("MeshTrailer")]
    public Variable<bool> isTrailActive;

    [Header("Resources")]
    protected float initialMaxHealth = 100;
    protected float initialStamina = 100;
    
    public ClampedVariable<float> health;
    public ClampedVariable<float> stamina;
    public ClampedVariable<int> actionPoint;

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
        CLVM = GetComponent<CharacterLocomotionVariableManager>();
    }

    public void SetInitialMaxHealth(int value)
    {
        initialMaxHealth = value;
    }

    protected virtual void Start()
    {
        if (!_isInitVariable)
        {
            InitVariable();
        }
    }

    private void FixedUpdate()
    {
        if(character.isDead.Value) return;
        if (groggy.Value < groggy.MaxValue)
        {
            groggy.Value += Time.deltaTime;
        }
    }

    public virtual void InitVariable()
    {
        if(_isInitVariable) return;
        _isInitVariable = true;

        health = new ClampedVariable<float>(initialMaxHealth);
        stamina = new ClampedVariable<float>(initialStamina);
    }

    // 패리 당한 대상을 인자로 보냄
    public virtual void SuccessParry(CharacterManager parriedTarget)
    {
        StartCoroutine(ActivateInvincibility(2f));
    }
    private IEnumerator ActivateInvincibility(float duration)
    {
        isInvulnerable.Value = true;
        yield return new WaitForSecondsRealtime(duration);
        isInvulnerable.Value = false;
    }

    public void OnIsChargingAttack(bool newValue)
    {
        character.animator.SetBool(_isChargingHash, isCharging.Value);
    }
    
    public void OnBlocking(bool newValue)
    {
        character.characterVariableManager.CLVM.alwaysStrafe = isBlock.Value;
        character.animator.SetBool(_isBlockingHash, isBlock.Value);
    }
    
    // 해당 함수는 health.OnDepleted 에 구독된 함수임
    public virtual void DeathProcess(float newValue)
    {
        character.isDead.Value = true;
        character.animator.SetBool(_isDeadHash, true); // critical로 죽을 때 애니메이션 변경
    }

    public virtual void OnGroggy(float newValue)
    {
        character.characterAnimatorManager.PlayTargetActionAnimation(
            character.characterAnimatorManager.groggy,
            true
            );
        character.isGroggy.Value = true;
        StartCoroutine(ResetGroggy());
    }
    
    private IEnumerator ResetGroggy()
    {
        yield return new WaitForSeconds(_groggyTime);
        
        character.characterVariableManager.groggy.Value =
            character.characterVariableManager.groggy.MaxValue;
        character.isGroggy.Value = false;
        character.animator.SetTrigger(_groggyHash);
    }
}

