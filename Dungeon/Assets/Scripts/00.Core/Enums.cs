using System.Collections;
using System.Collections.Generic;
using UnityEngine;







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
