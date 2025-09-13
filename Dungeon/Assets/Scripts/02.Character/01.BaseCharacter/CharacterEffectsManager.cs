using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffectsManager : MonoBehaviour
{
    CharacterManager character;

    [Header("VFX")]
    [SerializeField] GameObject bloodSplatterVFX;
    [SerializeField] GameObject blockVFX;
    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();   
    }
    public void ProcessInstantEffect(IInstantCharacterEffect effect)
    {
        effect.ProcessEffect(character);
    }

    public void PlayBloodSplatterVFX(Vector3 contactPoint)
    {
        if(bloodSplatterVFX != null)
        {
            Instantiate(bloodSplatterVFX, contactPoint, Quaternion.identity);
        }
        else
        {
            Instantiate(WorldCharacterEffectsManager.Instance.bloodSplatterVFX, contactPoint, Quaternion.identity);
        }
    }
    
    public void PlayBlockVFX(Vector3 contactPoint)
    {
        if(blockVFX != null)
        {
            Instantiate(blockVFX, contactPoint, Quaternion.identity);
        }
        else
        {
            Instantiate(WorldCharacterEffectsManager.Instance.blockVFX, contactPoint, Quaternion.identity);
        }
    }
}
