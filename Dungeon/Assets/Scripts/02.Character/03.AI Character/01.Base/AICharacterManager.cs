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
    [HideInInspector] public AICharacterPursueManager aiCharacterPursueManager;
    [HideInInspector] public AICharacterDeathInteractable aiCharacterDeathInteractable;
    
    [Header("Navmesh Agent")] 
    public NavMeshAgent navMeshAgent;
    
    [Header("CurrentState")] 
    [SerializeField] protected AIState currentState;
    
    [Space(10)]
    public IdleState stateIdle;
    public PursueTargetState statePursueTarget;
    public CombatStanceState stateCombatStance;
    public AttackState stateAttack;

    private Coroutine _actionRecoveryCoroutine;
    [HideInInspector] public bool isActionRecover = true;

    protected override void Awake()
    {
        base.Awake();

        aiCharacterVariableManager = GetComponent<AICharacterVariableManager>();
        aiCharacterCombatManager = GetComponent<AICharacterCombatManager>();
        aiCharacterLocomotionManager = GetComponent<AICharacterLocomotionManager>();
        aiCharacterPatrolManager = GetComponent<AICharacterPatrolManager>();
        aiCharacterPursueManager = GetComponent<AICharacterPursueManager>();
        
        aiCharacterDeathInteractable = GetComponentInChildren<AICharacterDeathInteractable>();
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
            characterVariableManager.health.OnValueChanged += characterUIManager.OnHPChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        if(characterUIManager && characterUIManager.hasFloatingHPBar)
            characterVariableManager.health.OnValueChanged -= characterUIManager.OnHPChanged;
    }
    
    private void SwitchToState(AIState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            currentState.OnEnterState(this);
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
    }

    protected override IEnumerator ProcessDeathEvent()
    {
        characterCollider.enabled = false;
        navMeshAgent.ResetPath();
        navMeshAgent.isStopped = true;
        //AISpawnManager.Instance?.NotifyTermination(this);
        return base.ProcessDeathEvent();
    }
    
    public void StartActionRecovery(float value)
    {
        if (_actionRecoveryCoroutine == null)
        {
            isActionRecover = false;
            _actionRecoveryCoroutine = StartCoroutine(ActionRecoveryCoroutine(value));
        }
    }

    private void StopActionRecovery()
    {
        if (_actionRecoveryCoroutine != null)
        {
            StopCoroutine(_actionRecoveryCoroutine);
            _actionRecoveryCoroutine = null;
        }
    }

    private IEnumerator ActionRecoveryCoroutine(float value)
    {
        yield return new WaitForSeconds(value);
        isActionRecover = true;
        _actionRecoveryCoroutine = null;
    }
}
