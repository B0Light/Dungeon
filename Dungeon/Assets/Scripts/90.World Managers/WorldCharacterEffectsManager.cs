using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldCharacterEffectsManager : Singleton<WorldCharacterEffectsManager>
{
    [Header("VFX")]
    public GameObject bloodSplatterVFX;
    public GameObject blockVFX;
    public GameObject parriedVFX;
    public GameObject healVFX;
    
    public List<GameObject> horizontalSwordSlashAura;
    public List<GameObject> verticalSwordSlashAura;
    public List<GameObject> swordPickAura;
    public List<GameObject> leapingAttackAura;
    
    [HideInInspector] public TakeDamageEffect takeDamageEffect;
    [HideInInspector] public TakeBlockDamageEffect takeBlockDamageEffect;
    [HideInInspector] public RestoreHealthEffect restoreHealthEffect;
    [HideInInspector] public EatingFoodEffect eatingFoodEffectEffect;
    [HideInInspector] public BuffAttackEffect buffAttackEffect;
    [HideInInspector] public BuffDefenseEffect buffDefenseEffect;
    [HideInInspector] public UtilitySpeedEffect utilitySpeedEffect;
    [HideInInspector] public UtilityWeightEffect utilityWeightEffect;
    [SerializeField] List<IInstantCharacterEffect> instantEffects;

    protected override void Awake()
    {
        base.Awake();
        takeDamageEffect = ScriptableObject.CreateInstance<TakeDamageEffect>();
        takeBlockDamageEffect = ScriptableObject.CreateInstance<TakeBlockDamageEffect>();
        restoreHealthEffect = ScriptableObject.CreateInstance<RestoreHealthEffect>();
        eatingFoodEffectEffect = ScriptableObject.CreateInstance<EatingFoodEffect>();
        buffAttackEffect = ScriptableObject.CreateInstance<BuffAttackEffect>();
        buffDefenseEffect = ScriptableObject.CreateInstance<BuffDefenseEffect>();
        utilitySpeedEffect = ScriptableObject.CreateInstance<UtilitySpeedEffect>();
        utilityWeightEffect = ScriptableObject.CreateInstance<UtilityWeightEffect>();
    }

    private void Start()
    {
        LoadAllSlashEffect();
    }

    private void LoadAllSlashEffect()
    {
        GameObject[] horizontalEffects = Resources.LoadAll<GameObject>("SlashEffects/01_Horizontal");
        foreach (var effect in horizontalEffects)
        {
            horizontalSwordSlashAura.Add(effect);
        }
        
        GameObject[] verticalEffects = Resources.LoadAll<GameObject>("SlashEffects/01_Horizontal");
        foreach (var effect in verticalEffects)
        {
            verticalSwordSlashAura.Add(effect);
        }
        
        GameObject[] pickEffects = Resources.LoadAll<GameObject>("SlashEffects/01_Horizontal");
        foreach (var effect in pickEffects)
        {
            swordPickAura.Add(effect);
        }
        
        GameObject[] leanAttackEffects = Resources.LoadAll<GameObject>("SlashEffects/01_Horizontal");
        foreach (var effect in leanAttackEffects)
        {
            leapingAttackAura.Add(effect);
        }
    }
    
    public void OnParrySuccess(Vector3 contactPoint)
    {
        Instantiate(parriedVFX, contactPoint, Quaternion.identity);
    }
    
    public void CastSwordSlash(Vector3 spawnPos, int weaponID, int attackType, CharacterManager castingCharacter)
    {
        GameObject castSwordSlash;
        int castID;
        switch (attackType)
        {
            case 0:
                if (horizontalSwordSlashAura.Count == 0) return;
                castID = weaponID < horizontalSwordSlashAura.Count ? weaponID : 0; 
                castSwordSlash = horizontalSwordSlashAura[castID];
                break;
            case 1:
                if (verticalSwordSlashAura.Count == 0) return;
                castID = weaponID < verticalSwordSlashAura.Count ? weaponID : 0;
                castSwordSlash = verticalSwordSlashAura[castID];
                break;
            case 2:
                if (swordPickAura.Count == 0) return;
                castID = weaponID < swordPickAura.Count ? weaponID : 0;
                castSwordSlash = swordPickAura[castID];
                break;
            case 3:
                if (leapingAttackAura.Count == 0) return;
                castID = weaponID < leapingAttackAura.Count ? weaponID : 0;
                castSwordSlash = leapingAttackAura[castID];
                break;
            default:
                if (horizontalSwordSlashAura.Count == 0) return;
                castSwordSlash = horizontalSwordSlashAura[0];
                break;
        }

        GameObject instanceSlash = Instantiate(castSwordSlash, spawnPos, castingCharacter.gameObject.transform.rotation);
        MeleeWeaponDamageCollider instanceDC = instanceSlash.GetComponent<MeleeWeaponDamageCollider>();
        instanceDC.ownerCharacter = castingCharacter;
        instanceDC.EnableDamageCollider();
    }
}
