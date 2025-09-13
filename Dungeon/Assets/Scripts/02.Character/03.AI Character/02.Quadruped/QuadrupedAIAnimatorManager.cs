using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedAIAnimatorManager : CharacterAnimatorManager
{
    #region AnimationHash

// Quadruped specific animation hashes
    private readonly int _attackReadyHash = Animator.StringToHash("AttackReady_b");
    private readonly int _attackTypeHash = Animator.StringToHash("AttackType_int");
    private readonly int _actionTypeHash = Animator.StringToHash("ActionType_int");
    private readonly int _turnAngleHash = Animator.StringToHash("TurnAngle_int");

// State booleans
    private readonly int _deathHash = Animator.StringToHash("Death_b");
    private readonly int _sleepHash = Animator.StringToHash("Sleep_b");
    private readonly int _sitHash = Animator.StringToHash("Sit_b");

    #endregion
    
    private Dictionary<StaticAnimationType, int> _animationHashes;
    
    private void Start()
    {
        InitializeAnimationHashes();
        
        hitForward  = "Damaged";
        hitBackward = "Damaged";
        hitLeft     = "Damaged";
        hitRight    = "Damaged";
    }
    
    private void InitializeAnimationHashes()
    {
        _animationHashes = new Dictionary<StaticAnimationType, int>
        {
            { StaticAnimationType.Death, _deathHash },
            { StaticAnimationType.Sleep, _sleepHash },
            { StaticAnimationType.Sit, _sitHash }
        };
    }

    private IEnumerator DogActions(int actionType)
    {
        characterManager.animator.SetInteger(_actionTypeHash, actionType);
        yield return new WaitForSeconds(1);
        characterManager.animator.SetInteger(_actionTypeHash, 0);
    }
}
