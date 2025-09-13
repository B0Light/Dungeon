using UnityEngine;

public class MutantAICombatManager : AICharacterCombatManager
{
    public void Explosion()
    {
        UnifiedProjectilePoolManager.Instance.FireInDirection(
            aiCharacter,
            ProjectileType.MutantBoom, 
            transform.position, 
            Vector3.zero,
            transform
        );

        character.characterVariableManager.health.Value -= 50;
    }
}
