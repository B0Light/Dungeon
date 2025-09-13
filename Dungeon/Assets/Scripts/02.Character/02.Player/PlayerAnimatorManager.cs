using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAnimatorManager : CharacterAnimatorManager
{
    private PlayerManager _playerManager;

    protected override void Awake()
    {
        base.Awake();

        _playerManager = characterManager as PlayerManager;
    }

    private void OnAnimatorMove()
    {
        if(_playerManager.isDead.Value) return;
        if(!applyRootMotion) return;
        
        Vector3 velocity = _playerManager.animator.deltaPosition;
        _playerManager.characterController.Move(velocity);
        _playerManager.transform.rotation *= _playerManager.animator.deltaRotation;
    }
}
