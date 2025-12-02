using System.Collections;
using UnityEngine;

public class AdventurerAILocomotionManager : AICharacterLocomotionManager
{
    private DungeonManager _dungeonManager;
    // update 에서 실행  
    protected override Vector3 DetermineTargetPosition()
    {
        if (aiCharacterManager.CurrentTarget)
        {
            navAgent.stoppingDistance = aiCharacterManager.detectionRange;
            return aiCharacterManager.CurrentTarget.transform.position;
        }
        else
        {
            Vector3 waypoint = aiCharacterManager.aiCharacterPatrolManager.GetNextWaypoint();
            navAgent.stoppingDistance = 0.5f; // 순찰시 기본 정지 거리
            return waypoint;
        }
    }

    private DungeonManager GetDungeonManager()
    {
        if(_dungeonManager == null) 
            _dungeonManager = FindFirstObjectByType<DungeonManager>();
        return _dungeonManager;
    }

    private IEnumerator SearchTarget()
    {
        yield return new WaitForSeconds(5f);
    }
}
