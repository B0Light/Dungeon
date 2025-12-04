using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectsManager : CharacterEffectsManager
{
    [Header("Debug Delete Later")]
    [SerializeField] IInstantEffect effectToTest;
    [SerializeField] bool processEffect = false;

    private void Update()
    {
        if (processEffect)
        {
            processEffect = false;
            IInstantEffect effect = Instantiate(effectToTest);
            ProcessInstantEffect(effect);
        }
    }
}
