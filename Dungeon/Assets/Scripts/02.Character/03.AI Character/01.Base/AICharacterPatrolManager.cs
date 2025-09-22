using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI 캐릭터의 패트롤 행동을 관리하는 매니저 (AIPatrolManager 기능 통합)
/// </summary>
public class AICharacterPatrolManager : MonoBehaviour
{
    public Vector3 GetNextWaypoint()
    {
        return new Vector3();
    }

    public bool IsInAmbushMode()
    {
        return true;
    }
}