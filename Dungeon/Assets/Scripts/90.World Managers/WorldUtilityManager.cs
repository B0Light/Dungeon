using UnityEngine;

public class WorldUtilityManager : Singleton<WorldUtilityManager>
{
    [Header("Layers")]
    [SerializeField] private LayerMask characterLayer;
    [SerializeField] private LayerMask envLayer;
    
    public enum CharacterGroup
    {
        Team01,
        Team02,
        Team03,
    }

    public LayerMask GetCharacterLayer()
    {
        return characterLayer;
    }

    public LayerMask GetEnvLayer()
    {
        return envLayer;
    }

    public bool CanIDamageThisTarget(CharacterGroup attackingCharacter, CharacterGroup targetCharacter)
    {
        if (attackingCharacter == CharacterGroup.Team01)
        {
            switch (targetCharacter)
            {
                case CharacterGroup.Team01: return false;
                case CharacterGroup.Team02: return true;
                default: break;
            }
        }
        else if (attackingCharacter == CharacterGroup.Team02)
        {
            switch (targetCharacter)
            {
                case CharacterGroup.Team01: return true;
                case CharacterGroup.Team02: return false;
                default:  break;
            }
        }

        return false;
    }

    public float GetAngleOfTarget(Transform characterTransform, Vector3 targetDirection)
    {
        targetDirection.y = 0;
        float viewalbeAngle = Vector3.Angle(characterTransform.forward, targetDirection);
        Vector3 cross = Vector3.Cross(characterTransform.forward, targetDirection);

        if (cross.y < 0) viewalbeAngle = -viewalbeAngle;

        return viewalbeAngle;
    }
}
