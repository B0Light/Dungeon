using System;
using UnityEngine;

[Serializable]
public class ProjectileObject
{
    public ProjectileType projectileType;
    public GameObject projectilePrefab;
    public ParticleSystem particleSystemPrefab; // 파티클 시스템 프리팹
    public int initialPoolSize = 10;
    public float projectileSpeed = 20f;
    public float maxRange = 100f;
    public LayerMask collisionMask = -1;
}
