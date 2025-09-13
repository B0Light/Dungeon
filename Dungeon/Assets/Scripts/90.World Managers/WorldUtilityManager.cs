using UnityEngine;

public class WorldUtilityManager : Singleton<WorldUtilityManager>
{
    [Header("Layers")]
    [SerializeField] private LayerMask characterLayer;
    [SerializeField] private LayerMask envLayer;

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
    
    public DamageIntensity GetDamageIntensityBasedOnPoiseDamage(float poiseDamage)
    {
        //  THROWING DAGGERS, SMALL ITEMS, ETC, ETC...
        DamageIntensity damageIntensity = DamageIntensity.Ping;

        //  DAGGER / LIGHT ATTACKS
        if (poiseDamage >= 10)
            damageIntensity = DamageIntensity.Light;

        //  STANDARD WEAPONS / MEDIUM ATTACKS
        if (poiseDamage >= 30)
            damageIntensity = DamageIntensity.Medium;

        //  GREAT WEAPONS / HEAVY ATTACKS
        if (poiseDamage >= 70)
            damageIntensity = DamageIntensity.Heavy;

        //  ULTRA WEAPONS / COLOSSAL ATTACKS
        if (poiseDamage >= 120)
            damageIntensity = DamageIntensity.Colossal;

        return damageIntensity;
    }
}
