using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterSlot
{
    CharacterSlot_01,
    CharacterSlot_02,
    CharacterSlot_03,
    CharacterSlot_04,
    CharacterSlot_05,
    NO_SLOT
}

public enum CharacterGroup
{
    Team01,
    Team02,
    Team03,
}

public enum ItemGridType
{
    PlayerInventory,
    InteractableInventory,
    EquipmentInventory,
    ShareInventory,
    BackpackInventory,
    None,
}

public enum WeaponModelSlot
{
    RightHand,
    LeftHand,
    LeftChainsaw,
    //Right Hips,
    //Left Hips,
    //Back
}

public enum ItemType
{
    Weapon,
    Armor,
    Helmet,
    Consumables,
    Misc,
    None,
}

public enum ItemTier
{
    Common,     //0 white
    Uncommon,   //1 green  
    Rare,       //2 blue
    Epic,       //3 purple
    Legendary,  //4 orange
    Mythic,     //5 Red
    None,       // To out of index
}

public enum AttackType
{
    LightAttack01,
    LightAttack02,
    LightAttack03,
    HeavyAttack01,
    HeavyAttack02,
    HeavyAttack03,
    ChargeAttack01,
    ChargeAttack02,
    ChargeAttack03,
    Parry,
    Block,
    RunningAttack01,
    RollingAttack01,
    BackStepAttack01,
    JumpingAttack01,
    CriticalAttack,
    Skill,
}

public enum ItemEffect
{
    PhysicalAttack,     // 0. 물리 공격력 증가
    MagicalAttack,      // 1. 마법 공격력 증가
    PhysicalDefense,    // 2. 물리 방어력 증가
    MagicalDefense,     // 3. 마법 방어력 증가
    HealthPoint,        // 4. 최대 체력 증가
    RestoreHealth,      // 5. 체력 회복
    EatingFood,         // 6. 배고픔 회복
    BuffAttack,         // 7. 공격력 버프
    BuffDefense,        // 8. 방어력 버프
    BuffActionPoint,    // 9. 행동력 버프
    UtilitySpeed,       // 10. 이동속도 증가
    UtilityWeight,      // 11. 무게 감소
    Resource,           // 12. 자원 아이템
    StorageSpace,       // 13. 배낭 공간 확장
    None,               // 14. 효과 없음
}

public enum DamageIntensity
{
    Ping,
    Light,
    Medium,
    Heavy,
    Colossal,
}

public enum AnimationState
{
    Base,
    Locomotion,
    Jump,
    Fall,
    Crouch,
    DoubleJump,
    Dead,
}

public enum GaitState
{
    Idle,
    Walk,
    Run,
    Sprint,
}

// quadruped
public enum StaticAnimationType
{
    Death,
    Sleep,
    Sit
}

public enum BoxType
{
    WeaponBox,
    FoodBox,
    SupplyBox,
    MiscBox,
    Safe,
    
}

public enum TimeOfDay
{
    Day,
    Sunset,
    Night
}

public enum OptionType
{
    Display,
    KeyBind,
    Sound,
    Exit,
    None,
}

public enum ProjectileBehavior
{
    HitScan,    // 즉시 충돌 처리
    Physical,    // 물리적 이동
    Guided, // 새로 추가
}

public enum ProjectileType
{
    // 마법류
    Fireball,     // 화염 마법
    IceSpike,     // 얼음 창
    LightningBolt, // 번개
    PoisonDart,   // 독침
    ArcaneMissile, // 비전 미사일
    RockShard,     // 돌 파편
    WindSlash,     // 바람의 칼날
    ShadowOrb,      // 그림자 구체
    
    // 근접 무기 관련
    SwordSlash,    // 검기 (짧은 거리 베기)
    SwordWave,     // 장거리 검기파
    EnergyBlade,   // 에너지 칼날 발사체

    // 총기류
    Bullet,        // 일반 탄환
    ExplosiveRound,// 폭발탄
    ShotgunPellet, // 산탄
    SniperRound,   // 저격탄
    Rocket,        // 로켓탄
    Grenade,        // 투척형 수류탄
    
    // 보스공격
    BossJumpAttack,
    BossSmash,
    BossShockWave,
    MutantBoom,
    
    // 지면강타
    BDY_GroundSlam,
}

public enum StatusEffectType
{
    None,
    Poison,
    Burn,
    Freeze,
    Stun,
    Slow,
    Bleeding
}

public enum Difficulty
{
    [InspectorName("평화로움")]
    Easy = 0,
    
    [InspectorName("보통")]
    Normal = 1,
    
    [InspectorName("어려움")]
    Hard = 2,
    
    [InspectorName("전문가")]
    Expert = 3,
    
    [InspectorName("지옥")]
    Hell = 4,
}

