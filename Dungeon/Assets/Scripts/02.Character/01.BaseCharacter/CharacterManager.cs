using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour, IDamageable
{
    [Header("Status")]
    public Variable<bool> isDead = new Variable<bool>(false);
    public Variable<bool> isGroggy = new Variable<bool>(false);
    public Variable<bool> isBattle = new Variable<bool>(false);

    // 컴포넌트 레퍼런스
    [HideInInspector] public Animator animator;
    [HideInInspector] public Collider characterCollider;
    [HideInInspector] public CharacterVariableManager characterVariableManager;
    [HideInInspector] public CharacterEffectsManager characterEffectsManager;
    [HideInInspector] public CharacterAnimatorManager characterAnimatorManager;
    [HideInInspector] public CharacterCombatManager characterCombatManager;
    [HideInInspector] public CharacterStatsManager characterStatsManager;
    [HideInInspector] public CharacterSoundFXManager characterSoundFXManager;
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    [HideInInspector] public CharacterEquipmentMangaer characterEquipmentManager;
    [HideInInspector] public CharacterUIManager characterUIManager;
    [HideInInspector] public MeshTrail meshTrail;

    private Rigidbody _rigidbody;
    
    [Header(("CharacterGroup"))] 
    public WorldUtilityManager.CharacterGroup characterGroup;

    [HideInInspector] public bool isPerformingAction = false;
    [HideInInspector] public float attackRange = 5f;

    public CharacterManager CurrentTarget { get; private set; }

    public Vector3 lockOnPosition;
        
    #region Unity Lifecycle Methods
    
    protected virtual void Awake()
    {
        Debug.Log("Character Manager Awake");
        InitializeComponents();
    }

    protected virtual void Start()
    {
        IgnoreMyOwnColliders();
    }
    
    protected virtual void Update()
    {
        if(isDead.Value) return; 
        UpdateCharacterState();
    }
    
    protected virtual void OnEnable()
    {
        characterVariableManager.InitVariable();
        SubscribeToEvents();
    }
    
    protected virtual void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("NO ANIMATOR");
        }

        characterVariableManager = GetComponent<CharacterVariableManager>();
        characterEffectsManager = GetComponent<CharacterEffectsManager>();
        characterAnimatorManager = GetComponent<CharacterAnimatorManager>();
        characterCombatManager = GetComponent<CharacterCombatManager>();
        characterStatsManager = GetComponent<CharacterStatsManager>();
        characterSoundFXManager = GetComponent<CharacterSoundFXManager>();
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
        characterEquipmentManager = GetComponent<CharacterEquipmentMangaer>();
        characterUIManager = GetComponent<CharacterUIManager>();
        
        characterCollider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        meshTrail = GetComponent<MeshTrail>();
        
        characterAnimatorManager?.Spawn();
    }
    
    private void SubscribeToEvents()
    {
        isBattle.OnValueChanged += OnCharacterBattle;
        isDead.OnValueChanged += OnCharacterDeath;
        
        characterVariableManager.health.OnDepleted += characterVariableManager.DeathProcess;
        characterVariableManager.groggy.OnDepleted += characterVariableManager.OnGroggy;
        characterVariableManager.isBlock.OnValueChanged += characterVariableManager.OnBlocking;
        characterVariableManager.isCharging.OnValueChanged += characterVariableManager.OnIsChargingAttack;
        characterVariableManager.isTrailActive.OnValueChanged += meshTrail.OnTrailActiveChanged;
    }
    
    private void UnsubscribeFromEvents()
    {
        isBattle.OnValueChanged -= OnCharacterBattle;
        isDead.OnValueChanged -= OnCharacterDeath;
        
        characterVariableManager.health.OnDepleted -= characterVariableManager.DeathProcess;
        characterVariableManager.groggy.OnDepleted -= characterVariableManager.OnGroggy;
        characterVariableManager.isBlock.OnValueChanged -= characterVariableManager.OnBlocking;
        characterVariableManager.isCharging.OnValueChanged -= characterVariableManager.OnIsChargingAttack;
        characterVariableManager.isTrailActive.OnValueChanged -= meshTrail.OnTrailActiveChanged;
    }
    
    #endregion

    #region Character State Management
    
    private void UpdateCharacterState()
    {
        ActivateTrail();
        characterVariableManager.position.Value = transform.position;
        characterVariableManager.rotation.Value = transform.rotation;
    }

    private void OnCharacterBattle(bool value)
    {
        characterLocomotionManager.canLocomotion = !value;
        InputHandlerManager.Instance.SetInputMode(value ? InputMode.Combat : InputMode.Exploration);
        GUIController.Instance.CloseGUI();
    }
    
    private void OnCharacterDeath(bool value)
    {
        if (value)
        {
            StartCoroutine(ProcessDeathEvent());
        }
    }
    
    protected virtual IEnumerator ProcessDeathEvent()
    {
        yield return new WaitForFixedUpdate();
        characterVariableManager.health.Value = 0;
        SetTarget(null);
        characterAnimatorManager.PlayDeadAnimation();
    }
    
    protected virtual void ActivateTrail()
    {
        characterVariableManager.isTrailActive.Value =
            _rigidbody.linearVelocity.magnitude >= characterVariableManager.CLVM.sprintSpeed;
    }
    
    #endregion

    #region Combat and Damage

    public void SetTarget(CharacterManager newTarget)
    {
        if(newTarget == null || newTarget.isBattle.Value) return;
        CurrentTarget = newTarget;
    }

    public bool IsOpponent(WorldUtilityManager.CharacterGroup targetGroup)
    {
        return targetGroup != this.characterGroup;
    }

    public bool CanTakeDamage()
    {
        return !(characterVariableManager.isInvulnerable.Value || isDead.Value);
    }
    
    public void ProcessInstantEffect(TakeDamageEffect damageEffect)
    {
        characterEffectsManager.ProcessInstantEffect(damageEffect);
    }
    
    public void TakeDamage(float finalDamage, float poiseDamage)
    {
        characterVariableManager.health.Value -= (int)finalDamage;
        characterVariableManager.groggy.Value -= poiseDamage;
    }

    public void PostDamageEffect(Vector3 contactPoint, float angleHitFrom, bool isBlock)
    {
        if (isBlock)
        {
            // VFX
            characterEffectsManager.PlayBlockVFX(contactPoint);
            
            // SFX
            AudioClip blockSfx = WorldSoundFXManager.Instance.ChooseBlockSfx();

            if (blockSfx != null)
                characterSoundFXManager.PlaySoundFX(blockSfx);
            else
                characterSoundFXManager.PlayDamageGruntSoundFX();
        }
        else
        {
            // VFX
            characterEffectsManager.PlayBloodSplatterVFX(contactPoint);

            //SFX
            AudioClip physicalDamageSfx = WorldSoundFXManager.Instance.ChoosePhysicalDamageSfx();

            if (physicalDamageSfx != null)
                characterSoundFXManager.PlaySoundFX(physicalDamageSfx);
            else
                characterSoundFXManager.PlayDamageGruntSoundFX();
        }

        if(isDead.Value) return;
        characterAnimatorManager.PlayDirectionalHitAnimation(angleHitFrom, isBlock);
    }
    
    
    
    public float GetPhysicalAbsorption(bool isBlock)
    {
        float basePhysicalAbsorption = characterStatsManager.basePhysicalAbsorption + characterStatsManager.extraPhysicalAbsorption;
        return basePhysicalAbsorption + (isBlock ? characterStatsManager.blockingPhysicalAbsorption : 0);
    }
    

    public float GetMagicalAbsorption(bool isBlock)
    {
        float baseMagicalAbsorption = characterStatsManager.baseMagicalAbsorption + characterStatsManager.extraMagicalAbsorption;
        return baseMagicalAbsorption + (isBlock ? characterStatsManager.blockingMagicalAbsorption : 0);
    }

    #endregion

    #region Collision Management
    
    private void IgnoreMyOwnColliders()
    {
        Collider[] damageableCharacterColliders = GetComponentsInChildren<Collider>();
        List<Collider> ignoreColliders = new List<Collider>(damageableCharacterColliders);
        ignoreColliders.Add(characterCollider);

        foreach (var mainCollider in ignoreColliders)
        {
            foreach (var otherCollider in ignoreColliders)
            {
                Physics.IgnoreCollision(mainCollider, otherCollider, true);
            }
        }
    }
    
    #endregion
}