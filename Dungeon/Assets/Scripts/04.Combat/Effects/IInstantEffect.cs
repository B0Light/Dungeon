using UnityEngine;

public abstract class IInstantEffect : ScriptableObject
{
    public abstract void ProcessEffect(IEffectable effectTarget);
}
