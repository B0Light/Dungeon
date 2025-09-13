using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour, IDamageable
{
    [Header("Status")]
    public Variable<bool> isDead = new Variable<bool>(false);
    public Variable<bool> isGroggy = new Variable<bool>(false);

    public bool IsAlive => isDead.Value;

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
    public CharacterGroup characterGroup;

    [HideInInspector] public bool isPerformingAction = false;
     
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
        isDead.OnValueChanged += OnCharacterDeath;
        characterVariableManager.health.OnDepleted += characterVariableManager.DeathProcess;
        characterVariableManager.groggy.OnDepleted += characterVariableManager.OnGroggy;
        characterVariableManager.isBlock.OnValueChanged += characterVariableManager.OnBlocking;
        characterVariableManager.isCharging.OnValueChanged += characterVariableManager.OnIsChargingAttack;
        characterVariableManager.isTrailActive.OnValueChanged += meshTrail.OnTrailActiveChanged;
    }
    
    private void UnsubscribeFromEvents()
    {
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
    
    private void OnCharacterDeath(bool value)
    {
        if (isDead.Value)
        {
            StartCoroutine(ProcessDeathEvent());
        }
    }
    
    protected virtual IEnumerator ProcessDeathEvent()
    {
        yield return new WaitForFixedUpdate();
        characterVariableManager.health.Value = 0;
        characterCombatManager.SetTarget(null);
        characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true, canMove: false, canRotate: false);
    }
    
    protected virtual void ActivateTrail()
    {
        characterVariableManager.isTrailActive.Value =
            _rigidbody.linearVelocity.magnitude >= characterVariableManager.CLVM.sprintSpeed;
    }
    
    #endregion

    #region Combat and Damage
    
    public void TakeDamage(DamageData data)
    {
        if (characterVariableManager.isInvulnerable.Value || isDead.Value) return;

        // TakeDamageEffect 클래스를 활용하여 데미지 처리
        TakeDamageEffect damageEffect = ScriptableObject.CreateInstance<TakeDamageEffect>();
        damageEffect.SetDamage(data.attacker, data.physicalDamage, data.magicalDamage, data.extraDamage, 
                               data.poiseDamage, data.contactPoint, data.angleHitFrom);
        damageEffect.ProcessEffect(this);
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