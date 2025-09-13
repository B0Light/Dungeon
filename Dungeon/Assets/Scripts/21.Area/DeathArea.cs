using UnityEngine;

public class DeathArea : Area
{
    protected override void EnterArea(CharacterManager character)
    {
        character.isDead.Value = true;
    }

    protected override void ExitArea(CharacterManager character)
    {
        
    }
}
