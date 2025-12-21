using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_Combat : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    
    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    public void Register(PlayerControls playerControls)
    {
        Debug.Log("[Register] InputHandler_Combat");
        playerControls.PlayerCombat.Enable();
        playerControls.PlayerCombat.LightAttack.performed += OnLightAttack;
        playerControls.PlayerCombat.HeavyAttack.performed += OnHeavyAttack;
        playerControls.PlayerCombat.ChargeAttack.performed += OnChargeAttack;
        playerControls.PlayerCombat.ChargeAttack.canceled += CloseChargeAttack;

        playerControls.PlayerCombat.Parry.performed += OnParry;
        playerControls.PlayerCombat.Block.performed += OnBlock;
        playerControls.PlayerCombat.Block.canceled += CloseBlock;
        playerControls.PlayerCombat.Skill.performed += OnSkill;
    }

    public void Unregister(PlayerControls playerControls)
    {
        Debug.Log("[Unregister] InputHandler_Combat");
        playerControls.PlayerCombat.LightAttack.performed -= OnLightAttack;
        playerControls.PlayerCombat.HeavyAttack.performed -= OnHeavyAttack;
        playerControls.PlayerCombat.ChargeAttack.performed -= OnChargeAttack;
        playerControls.PlayerCombat.ChargeAttack.canceled -= CloseChargeAttack;

        playerControls.PlayerCombat.Parry.performed -= OnParry;
        playerControls.PlayerCombat.Block.performed -= OnBlock;
        playerControls.PlayerCombat.Block.canceled -= CloseBlock;
        playerControls.PlayerCombat.Skill.performed -= OnSkill;
        playerControls.PlayerCombat.Disable();
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
        if(!context.performed) return;
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.LightAttack);
    }
    
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.HeavyAttack);
        _playerManager.playerVariableManager.isCharging.Value = false;
    }
    
    private void OnChargeAttack(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.HeavyAttack);
        _playerManager.playerVariableManager.isCharging.Value = true;
    }
    
    private void CloseChargeAttack(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.HeavyAttack);
        _playerManager.playerVariableManager.isCharging.Value = false;
    }
    
    private void OnParry(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        _playerManager.playerCombatManager.PerformWeaponBasedAction(AttackType.Block);
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
