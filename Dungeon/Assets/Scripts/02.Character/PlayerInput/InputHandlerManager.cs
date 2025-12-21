using System;
using System.Collections.Generic;
using UnityEngine;

public enum InputMode { Exploration, Combat, OpenUI, Exit }

public class InputHandlerManager : Singleton<InputHandlerManager>
{
    private PlayerManager _playerManager;
    private PlayerControls _playerControls;
    private readonly HashSet<IInputHandler> _activeHandlers = new HashSet<IInputHandler>();
    private InputMode _lastInputMode = InputMode.Exit;
    private InputMode _curInputMode;
    [SerializeField] private InputHandler_Locomotion movementHandler;
    [SerializeField] private InputHandler_Combat combatHandler;
    [SerializeField] private InputHandler_UI uiHandler;
    
    public interface IInputHandler
    {
        void SetPlayer(PlayerManager playerManager);
        void Register(PlayerControls playerControls);
        void Unregister(PlayerControls playerControls);
    }

    protected override void Awake()
    {
        base.Awake();
        _playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        WorldSceneChangeManager.OnSceneEndPhase += () => SetInputMode(InputMode.Exit);
    }

    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;

        foreach (var handler in _activeHandlers)
        {
            handler.SetPlayer(_playerManager);
        }
    }

    private void RegisterAndEnableHandler(IInputHandler handler)
    {
        if(handler == null) return;
        if (_activeHandlers.Add(handler))
        {
            handler.Register(_playerControls);
            handler.SetPlayer(_playerManager);
        }
    }
    
    public void UnregisterAndDisableHandler(IInputHandler handler)
    {
        if(handler == null) return;
        if (_activeHandlers.Remove(handler))
        {
            handler.Unregister(_playerControls);
        }
    }

    public void SetLastInputMode()
    {
        SetInputMode(_lastInputMode);
    }
    
    public void SetInputMode(InputMode mode)
    {
        if(_curInputMode == mode) return;
        _lastInputMode = _curInputMode;
        _curInputMode = mode;
        switch (mode)
        {
            case InputMode.Exploration:
                SetControlActive(true);
                RegisterAndEnableHandler(movementHandler); 
                UnregisterAndDisableHandler(combatHandler); 
                RegisterAndEnableHandler(uiHandler);
                break;
            case InputMode.Combat:
                SetControlActive(true);
                RegisterAndEnableHandler(movementHandler); 
                RegisterAndEnableHandler(combatHandler); 
                RegisterAndEnableHandler(uiHandler);
                break;
            case InputMode.OpenUI:
                SetControlActive(false);
                ResetLocomotion();
                UnregisterAndDisableHandler(movementHandler); 
                UnregisterAndDisableHandler(combatHandler); 
                RegisterAndEnableHandler(uiHandler);
                break;
            case InputMode.Exit:
                SetControlActive(false);
                UnregisterAndDisableHandler(movementHandler); 
                UnregisterAndDisableHandler(combatHandler); 
                UnregisterAndDisableHandler(uiHandler);
                break;
        }
    }
    
    private void SetControlActive(bool isActive)
    {
        //Debug.Log($"SetControlActive : {isActive}");
            
        if(_playerManager != null)
            _playerManager.playerVariableManager.canControl.Value = isActive;
    }

    private void ResetLocomotion()
    {
        _playerManager.playerVariableManager.CLVM.moveDirection = Vector3.zero;
    }

    private void OnDisable()
    {
        SetInputMode(InputMode.Exit);
    }
}
