using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_Combat : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private PlayerControls _playerControls;
    
    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    private void Start()
    {
        InputHandlerManager.Instance.RegisterAndEnableHandler(this);
    }
    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    public void EnableInput()
    {
        _playerControls.PlayerCombat.Enable();
        _playerControls.PlayerCombat.LightAttack.performed += OnLightAttack;
        _playerControls.PlayerCombat.HeavyAttack.performed += OnHeavyAttack;
        _playerControls.PlayerCombat.ChargeAttack.performed += OnChargeAttack;
        _playerControls.PlayerCombat.ChargeAttack.canceled += CloseChargeAttack;

        _playerControls.PlayerCombat.Parry.performed += OnParry;
        _playerControls.PlayerCombat.Block.performed += OnBlock;
        _playerControls.PlayerCombat.Block.canceled += CloseBlock;
        _playerControls.PlayerCombat.Skill.performed += OnSkill;
    }

    public void DisableInput()
    {
        _playerControls.PlayerCombat.Disable();
    }
    
    private void OnDestroy()
    {
        if (InputHandlerManager.Instance != null)
        {
            InputHandlerManager.Instance.UnregisterAndDisableHandler(this);
        }
    }


    private void OnLightAttack(InputAction.CallbackContext context)
    {
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.LightAttack01);
    }
    
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.HeavyAttack01);
        _playerManager.playerVariableManager.isCharging.Value = false;
    }
    
    private void OnChargeAttack(InputAction.CallbackContext context)
    {
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.ChargeAttack01);
        _playerManager.playerVariableManager.isCharging.Value = true;
    }
    
    private void CloseChargeAttack(InputAction.CallbackContext context)
    {
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.ChargeAttack01);
        _playerManager.playerVariableManager.isCharging.Value = false;
    }
    
    private void OnParry(InputAction.CallbackContext context)
    {
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.Parry);
    }
    
    private void OnBlock(InputAction.CallbackContext context)
    {
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.Block);
        _playerManager.playerVariableManager.isBlock.Value = true;
    }

    private void CloseBlock(InputAction.CallbackContext context)
    {
        _playerManager.playerVariableManager.isBlock.Value = false;
    }
    
    private void OnSkill(InputAction.CallbackContext context)
    {
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.Skill);
    }
}
