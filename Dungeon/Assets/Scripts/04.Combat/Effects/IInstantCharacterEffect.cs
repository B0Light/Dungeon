using UnityEngine;

public abstract class IInstantCharacterEffect : ScriptableObject
{
    public abstract void ProcessEffect(CharacterManager effectTarget);
}
