using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterManager : CharacterManager
{
    [Header("Character ID")] public int characterID = 0;
    [Header("Character Name")] public string characterName = "";
    
    [HideInInspector] public AICharacterVariableManager aiCharacterVariableManager;
    [HideInInspector] public AICharacterCombatManager aiCharacterCombatManager;
    [HideInInspector] public AICharacterLocomotionManager aiCharacterLocomotionManager;
    [HideInInspector] public AICharacterPatrolManager aiCharacterPatrolManager;
    
    [HideInInspector] public AICharacterDeathInteractable aiCharacterDeathInteractable;
    [HideInInspector] public LockOnObject lockOnObject;
    
    [Header("Navmesh Agent")] 
    public NavMeshAgent navMeshAgent;
    
    [Header("CurrentState")] 
    [SerializeField] protected AIState currentState;
    
    [Space(10)]
    public IdleState stateIdle;
    public PursueTargetState statePursueTarget;
    public CombatStanceState stateCombatStance;
    public AttackState stateAttack;

    private Coroutine actionRecoveryCoroutine;
    [HideInInspector] public bool isActionRecover = true;

    protected override void Awake()
    {
        base.Awake();

        aiCharacterVariableManager = GetComponent<AICharacterVariableManager>();
        aiCharacterCombatManager = GetComponent<AICharacterCombatManager>();
        aiCharacterLocomotionManager = GetComponent<AICharacterLocomotionManager>();
        aiCharacterPatrolManager = GetComponent<AICharacterPatrolManager>();
        aiCharacterDeathInteractable = GetComponentInChildren<AICharacterDeathInteractable>();
        lockOnObject = GetComponentInChildren<LockOnObject>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        stateIdle = Instantiate(stateIdle);
        statePursueTarget = Instantiate(statePursueTarget);
        stateCombatStance = Instantiate(stateCombatStance);
        stateAttack = Instantiate(stateAttack);
        SwitchToState(stateIdle);

        if (characterUIManager && characterUIManager.hasFloatingHPBar)
            characterVariableManager.health.OnValueChanged +=
                characterUIManager.OnHPChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        if(characterUIManager && characterUIManager.hasFloatingHPBar)
            characterVariableManager.health.OnValueChanged -=
                characterUIManager.OnHPChanged;
    }
    
    private void SwitchToState(AIState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            currentState.OnEnterState(this); // 새로운 상태의 초기화 메서드 호출
        }
    }
    
    private void FixedUpdate()
    {
        if(isDead.Value) return; 
        ProcessStateMachine();
    }
    private void ProcessStateMachine()
    {
        AIState nextState = currentState?.Tick(this);
        
        if (nextState != null)
        {
            SwitchToState(nextState);
        }

        if (aiCharacterCombatManager.currentTarget != null)
        {
            aiCharacterCombatManager.targetDirection =
                aiCharacterCombatManager.currentTarget.transform.position - transform.position;
            aiCharacterCombatManager.viewableAngle = 
                WorldUtilityManager.Instance.GetAngleOfTarget(transform, aiCharacterCombatManager.targetDirection);
            aiCharacterCombatManager.distanceFromTarget =
                Vector3.Distance(transform.position, aiCharacterCombatManager.currentTarget.transform.position);
        }
    }

    protected override IEnumerator ProcessDeathEvent()
    {
        characterCollider.enabled = false;
        navMeshAgent.ResetPath();
        navMeshAgent.isStopped = true;
        AISpawnManager.Instance?.NotifyTermination(this);
        return base.ProcessDeathEvent();
    }
    
    public void StartActionRecovery(float value)
    {
        if (actionRecoveryCoroutine == null)
        {
            isActionRecover = false;
            actionRecoveryCoroutine = StartCoroutine(ActionRecoveryCoroutine(value));
        }
    }

    private void StopActionRecovery()
    {
        if (actionRecoveryCoroutine != null)
        {
            StopCoroutine(actionRecoveryCoroutine);
            actionRecoveryCoroutine = null;
        }
    }

    private IEnumerator ActionRecoveryCoroutine(float value)
    {
        yield return new WaitForSeconds(value);
        isActionRecover = true;
        actionRecoveryCoroutine = null;
    }
}
