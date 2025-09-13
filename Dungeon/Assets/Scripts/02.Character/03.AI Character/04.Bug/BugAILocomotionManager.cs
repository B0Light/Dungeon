using HeneGames.BugController;
using UnityEngine;

public class BugAILocomotionManager : AICharacterLocomotionManager
{
    [SerializeField] private BugController bugController;
    [SerializeField] private Transform forwardDirection;
    
    [SerializeField] private float rotateSpeed = 3f;

    protected override void UpdateAnimatorController()
    {
        bugController.LookPos(navAgent.destination, rotateSpeed);
    }
}
