using UnityEngine;

public class FieldOfView_Circum : FieldOfView_Base
{
    protected override void FindTarget()
    {
        foreach (Transform target in _previouslyVisibleTargets)
        {
            if (target != null) 
            {
                ControlTargetViewMeshRenderer(target, false);
            }
        }
        _previouslyVisibleTargets.Clear();
        _visibleTarget.Clear();
        
        SetOrigin(transform.position);
        int hitCount = Physics.OverlapSphereNonAlloc(_origin, viewDistance, _targetColliders, searchLayerMask);
        for (int i = 0; i < hitCount; i++)
        {
            Transform target = _targetColliders[i].transform;
            Vector3 targetPosition = target.position;
            Vector3 dirToTarget = (targetPosition - _origin).normalized;
            
            ControlTargetViewMeshRenderer(target, true);
            float distanceToTarget = Vector3.Distance(_origin, targetPosition);
            if (!Physics.Raycast(_origin, dirToTarget, distanceToTarget, obstacleLayerMask))
            {
                _visibleTarget.Add(target); 
                Debug.Log($"FindTarget : {target.name}");
            }
            
        }
        
    }
}
